using System.Collections.Generic;
using IDEK.Tools.GameplayEssentials.Updating;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace IDEK.Tools.GameplayEssentials.Samples.HackSlash
{
    public struct TimeOfDayVisualEntry
    {
        public float percentInDay;
        public TimeOfDayVisualSet timeVisualData;
    }

    public struct TimeOfDayEventEntry
    {
        public float percentInDay;
        public TimeOfDayEvent timeEvent;

        public void Invoke(TimeOfDaySystem timeOfDaySystem)
        {
            timeEvent.Invoke(timeOfDaySystem);
        }
    }

    /// <summary>
    /// lets you make logic happen at a time of day?
    /// </summary>
    [CreateAssetMenu(menuName = "IDEK/TimeOfDay/TimeOfDayEvent", fileName = "TimeOfDayEvent", order = 0)]
    public class TimeOfDayEvent : ScriptableObject
    {
        public void Invoke(TimeOfDaySystem timeOfDaySystem)
        {
        }
    }
    
    /// <summary>
    /// Stores data used to set a time of day
    /// </summary>
    [CreateAssetMenu(menuName = "IDEK/TimeOfDay/TimeOfDayVisualPreset", fileName = "TimeOfDayVisualPreset", order = 0)]
    public class TimeOfDayVisualSet : ScriptableObject
    {
        public abstract class Property
        {
            public abstract void Lerp(float weight);
        }

        public Property[] properties;

        public void Lerp(float weight)
        {
            foreach (var prop in properties)
            {
                prop.Lerp(weight);
            }
        }
    }

    /// <summary>
    /// Marks a gameobject as being impactable by time of day
    /// </summary>
    public class TimeOfDayElementComponent : TickBehaviour
    {
        //it registers itself to the current active tod system so that tod system updates auto run this updater
        
        #region Overrides of TickBehaviour

        /// <inheritdoc />
        protected override void Tick(float deltaTickTime)
        {
        }

        #endregion
    }
    
    /// <summary>
    /// </summary>
    /// <remarks>
    /// TODO: Functionality for jumping to a specific time (need to hi-jack the animation controller somehow)
    /// TODO: Set timescale of each connected animatior via expected AnimationParams
    /// TODO: support for interacting with the DirectionaLight
    /// </remarks>
    public class TimeOfDaySystem : TickBehaviour
    {
        [FormerlySerializedAs("cycleDuration")] [Tooltip("Time of day in seconds (defaults to 20 minutes)")]
        public float lengthOfDay = 20 * 60;
        //some elements may not always be on in certain regions,
        //will likely have multiple animators set up for this system
        public List<Animator> dayNightAnimators; 
        public List<TimeOfDayVisualEntry> timeOfDayVisuals;
        public List<TimeOfDayEventEntry> timeOfDayEvents;
        public UnityEvent<int> newDayStartEvent;
        public bool startGameAtDayStart = true;

        private List<bool> _visualsTriggeredToday;
        private int _eventsTriggeredToday;
        public float CurrentTimeOfDay { get; protected set; } = 0f;
        public int CurrentDay { get; protected set; } = 0;

        private void Start()
        {
            if (startGameAtDayStart) newDayStartEvent.Invoke(CurrentDay);

            timeOfDayEvents.Sort((a, b) => a.percentInDay.CompareTo(b.percentInDay));
            timeOfDayVisuals.Sort((a, b) => a.percentInDay.CompareTo(b.percentInDay));
        }
        
        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            newDayStartEvent.AddListener(OnNewDayStart);
        }

        /// <inheritdoc />
        protected override void Tick(float deltaTickTime)
        {
            _TickClock(deltaTickTime);
            
            //check events 
            //if past/at time: invoke and increment to next
            if (_eventsTriggeredToday < timeOfDayEvents.Count && 
                CurrentTimeOfDay >= timeOfDayEvents[_eventsTriggeredToday].percentInDay * lengthOfDay)
            {
                //we invoke that upcoming event, then increment to target the next one
                timeOfDayEvents[_eventsTriggeredToday].timeEvent.Invoke(this);
                _eventsTriggeredToday++;
            }
        }

        protected void OnNewDayStart(int day)
        {
            _eventsTriggeredToday = 0;

        }

        private void _TickClock(float deltaTickTime)
        {
            CurrentTimeOfDay += deltaTickTime;
            if (CurrentTimeOfDay >= lengthOfDay)
            {
                CurrentDay++;
                newDayStartEvent.Invoke(CurrentDay);
            }
            CurrentTimeOfDay %= lengthOfDay;
        }
    }
}
