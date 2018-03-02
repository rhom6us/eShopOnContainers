using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

namespace Identity.API {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class FacebookPermission : IEnumerable<string> {
        private readonly IEnumerable<string> _scopes;

        private FacebookPermission(params string[] scopes) {
            _scopes = scopes;
        }

        private FacebookPermission(IEnumerable<string> scopes) : this(scopes.ToArray()) { }

        public static implicit operator string(FacebookPermission source) {
            return string.Join(" ", source._scopes.Distinct());
        }

        [NotNull]
        public static FacebookPermission operator +(FacebookPermission left, FacebookPermission right) {
            return new FacebookPermission(left._scopes.Concat(right._scopes));
        }

        /*user_actions.books,
        user_actions.fitness,
        user_actions.music,
        user_actions.news,
        user_actions.video,
        user_actions:{app_namespace},*/

        public static readonly FacebookPermission public_profile = new FacebookPermission("public_profile");
        public static readonly FacebookPermission user_friends = new FacebookPermission("user_friends");
        public static readonly FacebookPermission email = new FacebookPermission("email");
        public static readonly FacebookPermission publish_actions = new FacebookPermission("publish_actions");
        public static readonly FacebookPermission user_about_me = new FacebookPermission("user_about_me");

        public static readonly FacebookPermission user_birthday = new FacebookPermission("user_birthday");
        public static readonly FacebookPermission user_education_history = new FacebookPermission("user_education_history");
        public static readonly FacebookPermission user_events = new FacebookPermission("user_events");
        public static readonly FacebookPermission user_games_activity = new FacebookPermission("user_games_activity");
        public static readonly FacebookPermission user_groups = new FacebookPermission("user_groups");
        public static readonly FacebookPermission user_hometown = new FacebookPermission("user_hometown");
        public static readonly FacebookPermission user_likes = new FacebookPermission("user_likes");
        public static readonly FacebookPermission user_location = new FacebookPermission("user_location");
        public static readonly FacebookPermission user_managed_groups = new FacebookPermission("user_managed_groups");
        public static readonly FacebookPermission user_photos = new FacebookPermission("user_photos");
        public static readonly FacebookPermission user_posts = new FacebookPermission("user_posts");
        public static readonly FacebookPermission user_relationships = new FacebookPermission("user_relationships");
        public static readonly FacebookPermission user_relationship_details = new FacebookPermission("user_relationship_details");
        public static readonly FacebookPermission user_religion_politics = new FacebookPermission("user_religion_politics");
        public static readonly FacebookPermission user_status = new FacebookPermission("user_status");
        public static readonly FacebookPermission user_tagged_places = new FacebookPermission("user_tagged_places");
        public static readonly FacebookPermission user_videos = new FacebookPermission("user_videos");
        public static readonly FacebookPermission user_website = new FacebookPermission("user_website");
        public static readonly FacebookPermission user_work_history = new FacebookPermission("user_work_history");
        public static readonly FacebookPermission read_custom_friendlists = new FacebookPermission("read_custom_friendlists");
        public static readonly FacebookPermission read_insights = new FacebookPermission("read_insights");
        public static readonly FacebookPermission read_audience_network_insights = new FacebookPermission("read_audience_network_insights");
        public static readonly FacebookPermission read_mailbox = new FacebookPermission("read_mailbox");
        public static readonly FacebookPermission read_page_mailboxes = new FacebookPermission("read_page_mailboxes");
        public static readonly FacebookPermission read_stream = new FacebookPermission("read_stream");
        public static readonly FacebookPermission manage_notifications = new FacebookPermission("manage_notifications");
        public static readonly FacebookPermission manage_pages = new FacebookPermission("manage_pages");
        public static readonly FacebookPermission publish_pages = new FacebookPermission("publish_pages");
        public static readonly FacebookPermission rsvp_event = new FacebookPermission("rsvp_event");
        public static readonly FacebookPermission pages_show_list = new FacebookPermission("pages_show_list");
        public static readonly FacebookPermission pages_manage_cta = new FacebookPermission("pages_manage_cta");
        public static readonly FacebookPermission pages_manage_instant_articles = new FacebookPermission("pages_manage_instant_articles");
        public static readonly FacebookPermission ads_read = new FacebookPermission("ads_read");
        public static readonly FacebookPermission ads_management = new FacebookPermission("ads_management");
        public static readonly FacebookPermission business_management = new FacebookPermission("business_management");
        public static readonly FacebookPermission pages_messaging = new FacebookPermission("pages_messaging");
        public static readonly FacebookPermission pages_messaging_subscriptions = new FacebookPermission("pages_messaging_subscriptions");
        public static readonly FacebookPermission pages_messaging_payments = new FacebookPermission("pages_messaging_payments");
        public static readonly FacebookPermission pages_messaging_phone_number = new FacebookPermission("pages_messaging_phone_number");
        public static readonly FacebookPermission instagram_basic = new FacebookPermission("instagram_basic");
        public static readonly FacebookPermission instagram_manage_comments = new FacebookPermission("instagram_manage_comments");
        public static readonly FacebookPermission instagram_manage_insights = new FacebookPermission("instagram_manage_insights");

        public IEnumerator<string> GetEnumerator() {
            return _scopes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}
