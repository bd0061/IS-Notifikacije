using System.Net;

namespace MailBackend
{
    public class ContainerNameResolver
    {
        public static string Resolve(string containername)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(containername);
            IPAddress[] addressList = hostInfo.AddressList;
            return addressList[0].ToString();
        }
    }
}
