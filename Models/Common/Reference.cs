using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class Reference<T>
    {
        public List<T> Data { get; set; }
    }

    public class ReferenceItem : Entity
    {
        public string id { get; set; }
        public string value { get; set; }
    }
    public class Country : ReferenceItem
    {
        public string phonecode { get; set; }
    }
}