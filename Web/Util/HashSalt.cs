using System.ComponentModel.DataAnnotations;

namespace iread_identity_ms.Web.Util
{
    public class HashSalt
    {
        public string Hash { get; set; }
        public byte[] Salt { get; set; }
    }
}