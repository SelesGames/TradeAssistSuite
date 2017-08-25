using System.Text;

namespace Shared.Http
{
    class UriParametersBuilder
    {
        readonly StringBuilder sb;

        public UriParametersBuilder(string parameters = null)
        {
            sb = new StringBuilder(parameters);
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public void AddParameters(params (string, object)[] parameters)
        {
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    sb.Append(
                        $"{GetCorrectParameterSeparator()}{item.Item1}={item.Item2}"
                    );
                }
            }

            string GetCorrectParameterSeparator()
            {
                return sb.Length == 0 ? "?" : "&";
            }
        }
    }
}
