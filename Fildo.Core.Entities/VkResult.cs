using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.Entities
{
    public class VkResult
    {
        public List<SongVK> Songs { get; set; }
        public bool Error { get; set; }
        public string CaptchaUrl { get; set; }
        public string CaptchaSid { get; set; }
        public string CaptchaKey { get; set; }
        public string PreviousSearch { get; set; }

    }
}
