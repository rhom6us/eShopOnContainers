using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UserActions.Api.Webhooks.Facebook
{

    //LocationActivitiesLikes
    public class PageValue
    {
        public string page { get; set; }
    }


    //Friends
    public class UidValue
    {
        public string uid { get; set; }
    }

    //Photos
    public class ObjectVerb
    {
        public string verb { get; set; }
        public string object_id { get; set; }
    }

    //status
    // String



    [DataContract]
    public class FacebookWebhookUpdateBindingModel<TValue>
    {


        [DataMember(Name = "object", IsRequired = false)]
        public FacebookSubscriptionEventType Object { get; set; }

        [DataMember(Name = "entry")]
        public EntryBindingModel<TValue>[] Entries { get; set; }
    }

    [DataContract]
    public enum FacebookSubscriptionEventType
    {
        [EnumMember(Value = "user")]
        User,
        [EnumMember(Value = "page")]
        Page,
        [EnumMember(Value = "permissions")]
        Permissions,
        [EnumMember(Value = "payments")]
        Payments
    }

    [DataContract]
    public class EntryBindingModel<TValue>
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "changed_fields")]
        public string[] ChangedFields { get; set; }

        [DataMember(Name = "changes", IsRequired = false)]
        public EntryChangeaBindingModel<TValue>[] Changes { get; set; }


        [DataMember(Name = "time")]
        private int Time { get; set; }

        [IgnoreDataMember]
        public DateTimeOffset DateTime => DateTimeOffset.FromUnixTimeSeconds(Time);
    }

    [DataContract]
    public class EntryChangeaBindingModel<TValue>
    {
        [DataMember(Name = "field")]
        public string Field { get; set; }

        [DataMember(Name = "value", IsRequired = false)]
        public TValue Value { get; set; }

        [DataMember(Name = "verb", IsRequired = false)]
        public string Verb { get; set; }
    }


}
