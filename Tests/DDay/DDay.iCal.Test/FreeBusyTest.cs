using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Interfaces;
using KwasantICS.DDay.iCal.Interfaces.Components;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class FreeBusyTest
    {
        private string tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = "US-Eastern";
        }
        
        /// <summary>
        /// Ensures that GetFreeBusyStatus() return the correct status.
        /// </summary>
        [Test, Category("DDay")] //Category(("FreeBusy")]
        public void GetFreeBusyStatus1()
        {
            IICalendar iCal = new iCalendar();

            IEvent evt = iCal.Create<DDayEvent>();
            evt.Summary = "Test event";
            evt.Start = new iCalDateTime(2010, 10, 1, 8, 0, 0);
            evt.End = new iCalDateTime(2010, 10, 1, 9, 0, 0);

            IFreeBusy freeBusy = iCal.GetFreeBusy(new iCalDateTime(2010, 10, 1, 0, 0, 0), new iCalDateTime(2010, 10, 7, 11, 59, 59));
            Assert.AreEqual(FreeBusyStatus.Free, freeBusy.GetFreeBusyStatus(new iCalDateTime(2010, 10, 1, 7, 59, 59)));
            Assert.AreEqual(FreeBusyStatus.Busy, freeBusy.GetFreeBusyStatus(new iCalDateTime(2010, 10, 1, 8, 0, 0)));
            Assert.AreEqual(FreeBusyStatus.Busy, freeBusy.GetFreeBusyStatus(new iCalDateTime(2010, 10, 1, 8, 59, 59)));
            Assert.AreEqual(FreeBusyStatus.Free, freeBusy.GetFreeBusyStatus(new iCalDateTime(2010, 10, 1, 9, 0, 0)));
        }
    }
}
