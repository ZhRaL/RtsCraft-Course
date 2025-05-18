using Commands;
using EventBus;

namespace Events
{
    public struct ActionSelectedEvent : IEvent
    {
        public ActionBase Action { get; }

        public ActionSelectedEvent(ActionBase action)
        {
            Action = action;
        }
    }
}