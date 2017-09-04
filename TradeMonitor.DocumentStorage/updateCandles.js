// stored procedure for iterating over trades and updating candles
function updateCandles(candleSize) {
    var context = getContext();
    var collection = context.getCollection();
    var response = context.getResponse();

    var state;
    var updatedto;
    var currentCandleEndTime;

    var run = collection.filter(function (doc) { return doc.id == "state"; }, onStateLoaded);
    if (!run) throw new Error('Unable to retrieve the "state" document.');

    function onStateLoaded(error, resources, options) {
        if (error) throw error;
        // resources is an array.  iterate through it
        state = resources[0];
        updatedTo = state.updatedTo;

        var run2 = collection.filter(function (doc) { return doc.time > updatedTo }, processDocuments);
        if (!run2) throw new Error('Unable to query for matching trades.');
    }

    function processDocuments(error, resources, options) {
        if (error) throw error;

        var candleList = [];
        // create candles, add to list of candles
        processTrades(resources, candleList);
        response.setBody(JSON.stringify(list));
    }

    function processTrades(trades, list) {
        var firstPrice = 0;
        var lastPrice = 0;
        var localLow = Number.MAX_VALUE;
        var localHigh = 0;
        var unixEpochTimeFirst = Number.MAX_SAFE_INTEGER;
        var unixEpochTimeLast = Number.MIN_SAFE_INTEGER;
        var firstId = 0;
        var lastId = 0;
        var aggregateVolume = 0;
        
        var item;

        for (item of trades) {
            if (checkIfNewCandle(item)) {
                list.push({
                    id: "",
                    market: "",
                    candleSize: 60,  // representing 1 minute in this example
                    open: firstPrice,
                    close: lastPrice,
                    high: localHigh,
                    low: localLow,
                    volume: aggregateVolume,
                    firstTradeTime: unixEpochTimeFirst,
                    lastTradeTime: unixEpochTimeLast,
                    firstId: firstId,
                    lastId: lastId,
                    candleBeginTime: 0
                });

                // reset the counter variables
                firstPrice = 0;
                lastPrice = 0;
                localLow = Number.MAX_VALUE;
                localHigh = 0;
                unixEpochTimeFirst = Number.MAX_SAFE_INTEGER;
                unixEpochTimeLast = Number.MIN_SAFE_INTEGER;
                firstId = 0;
                lastId = 0;
                aggregateVolume = 0;
            }


            var rate = item.price;
            var quantity = item.quantity;

            // Update local low and local high price
            if (rate < localLow)
                localLow = rate;

            if (rate > localHigh)
                localHigh = rate;


            // Update the first and last prices
            var timeStamp = item.time;
            var id = item.id;


            if (timeStamp < unixEpochTimeFirst) {
                unixEpochTimeFirst = timeStamp;
                firstPrice = rate;
                firstId = id;
            }

            if (timeStamp > unixEpochTimeLast) {
                unixEpochTimeLast = timeStamp;
                lastPrice = rate;
                lastId = id;
            }

            // Update the aggregate volume
            aggregateVolume += quantity;
        }
    }

    function checkIfNewCandle(item) {
        if (item.time < currentCandleEndTime) {
            return false;
        }
        else {
            currentCandleEndTime = currentCandleEndTime + candleSize;
            return true;
        }
    }
}