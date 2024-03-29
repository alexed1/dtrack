﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Data.Interfaces;
using Data.States;
using Data.States.Templates;
using StructureMap;
using Utilities;

namespace Data.Entities
{
    public class EmailDO : BaseDO, IEmailDO
    {
        public EmailDO()
        {
            Recipients = new List<RecipientDO>();
            Attachments = new List<AttachmentDO>();
            Events = new List<EventDO>();
            DateReceived = DateTimeOffset.UtcNow;
            Envelopes = new List<EnvelopeDO>();

            //By default, the MessageID is a random GUID. This is so we can match our sent emails to replies
            SetMessageID(Guid.NewGuid().ToString());
        }


        public void TagEmailToBookingRequest(BookingRequestDO bookingRequestDO, bool linkToConversation = true)
        {
            SetReplyTo(bookingRequestDO.Id);
            AddReference(bookingRequestDO.MessageID);
            if (linkToConversation)
                ConversationId = bookingRequestDO.Id;
        }

        private void SetReplyTo(int bookingRequestID)
        {
            var injectReplyTo = ObjectFactory.GetInstance<IConfigRepository>().Get("injectReplyTo", true);
            
            ReplyToName = ObjectFactory.GetInstance<IConfigRepository>().Get("replyToName", String.Empty);
            string replyToEmail = ObjectFactory.GetInstance<IConfigRepository>().Get("replyToEmail", String.Empty);
            if (String.IsNullOrEmpty(replyToEmail))
                return;

            if (injectReplyTo)
            {
                var originalAddress = replyToEmail;
                var splitPoint = originalAddress.Split('@');
                var replyToAddress = String.Format("{0}+{1}@{2}", splitPoint[0], bookingRequestID, splitPoint[1]);
                ReplyToAddress = replyToAddress;
            }
            else
            {
                ReplyToAddress = replyToEmail;
            }
        }

        private void SetMessageID(String messageID)
        {
            const string messageFormat = @"<{0}@sant.com>";
            MessageID = String.Format(messageFormat, messageID);
        }

        private void AddReference(String messageID)
        {  
            if (String.IsNullOrEmpty(References))
                References = messageID;
            else
                References = string.Concat(References, "\t", messageID);
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// This is taken from the header in emails.
        /// We use it to group messages from clients into conversation threads
        /// We also use it to identify negotiation response emails
        /// *** Use 'SetMessageID' unless you know what you're doing! ***
        /// </summary>
        public String MessageID { get; set; }

        /// <summary>
        /// This is used at the moment for outbound emails.
        /// We don't store this for incoming emails - however, we do check the header of the same name
        /// It allows us to specify what thread the email belongs to.
        /// For example, an email about negotiation requests references the original booking
        /// In this situation, 
        /// MessageID = Guid.NewGuid().ToString();
        /// References = negotiationRequestDO.BookingRequest.MessageID;
        /// *** Use 'AddReference' unless you know what you're doing! ***
        /// </summary>
        public String References { get; set; }

        public String Subject { get; set; }
        public String HTMLText { get; set; }
        public String PlainText { get; set; }
        public DateTimeOffset DateReceived { get; set; }
        
        [ForeignKey("Conversation")]
        public int? ConversationId { get; set; }
        public virtual BookingRequestDO Conversation { get; set; }

        [ForeignKey("EmailStatusTemplate")]
        public int? EmailStatus { get; set; }
        public _EmailStatusTemplate EmailStatusTemplate { get; set; }

        [ForeignKey("From"), Required]
        public int? FromID { get; set; }
        public virtual EmailAddressDO From { get; set; }

        /// <summary>
        /// Overrides the name of the sender (instead of taking it from From.Name).
        /// </summary>
        public String FromName { get; set; }

        /// <summary>
        /// We do not link to email address rows - as we will be dynamically creating reply-tos based on entities
        /// No need to flood the email table
        /// </summary>
        public String ReplyToName { get; set; }
        public String ReplyToAddress { get; set; }


        /// <summary>
        /// This is guaranteed to contain 0..1 envelopes, never many. 
        /// </summary>
        [InverseProperty("Email")]
        public virtual List<EnvelopeDO> Envelopes { get; set; }

        [InverseProperty("Email")]
        public virtual List<RecipientDO> Recipients { get; set; }

        [InverseProperty("Email")]
        public virtual List<AttachmentDO> Attachments { get; set; }

        [InverseProperty("Emails")]
        public virtual List<EventDO> Events { get; set; }

        public IEnumerable<EmailAddressDO> To
        {
            get
            {
                return Recipients.Where(eea => eea.EmailParticipantType == EmailParticipantType.To).Select(eea => eea.EmailAddress).ToList();
            }
        }

        public IEnumerable<EmailAddressDO> BCC
        {
            get
            {
                return Recipients.Where(eea => eea.EmailParticipantType == EmailParticipantType.Bcc).Select(eea => eea.EmailAddress).ToList();
            }
        }

        public IEnumerable<EmailAddressDO> CC
        {
            get
            {
                return Recipients.Where(eea => eea.EmailParticipantType == EmailParticipantType.Cc).Select(eea => eea.EmailAddress).ToList();
            }
        }

        public void AddEmailRecipient(int type, EmailAddressDO emailAddress)
        {
            var newLink = new RecipientDO
            {
                Email = this,
                EmailAddress = emailAddress,
                EmailAddressID = emailAddress.Id,
                EmailID = Id,
                EmailParticipantType = type
            };

            Recipients.Add(newLink);
            emailAddress.Recipients.Add(newLink);
        }

    }
}
