/****************
 * 
 * Copyright (c) 2014-2016 ImmotionAR, a division of Beps Engineering.
 * All rights reserved
 * 
 * See licensing terms of this file in document <Assets folder>\ImmotionRoomUnity\License\LICENSE.TXT
 * 
 ****************/

namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Object = System.Object;

    public abstract class EventBasedMonoBehaviour : MonoBehaviour
    {
        #region Private fields

        // see http://stackoverflow.com/questions/22513881/unity3d-how-to-process-events-in-the-correct-thread
        private static readonly Object QueueLock = new Object();
        private readonly List<Action> m_QueuedEvents;
        private readonly List<Action> m_ExecutingEvents;
        private bool m_destroying = false; //true if the behaviour has already received the OnDestroy event
        #endregion

        #region Constructor

        protected EventBasedMonoBehaviour()
        {
            m_QueuedEvents = new List<Action>();
            m_ExecutingEvents = new List<Action>();
        }

        #endregion

        #region Methods

        protected void Update()
        {
            //don't handle events if we are destroying the behaviour
            if(!m_destroying)
                HandleEvents();
        }

        /// <summary>
        /// Called at behaviour destroyal: MUST CALL THIS IN ALL SUBCLASSES
        /// </summary>
        protected virtual void OnDestroy()
        {
            //the behaviour is destroying itself
            m_destroying = true;

            //clear all events queued
            lock (QueueLock)
            {
                m_QueuedEvents.Clear();
                m_ExecutingEvents.Clear();
            }

        }

        #endregion

        #region Protected methods

        protected void ExecuteOnUI(Action action)
        {
            if (m_destroying)
                return;

            if (m_QueuedEvents == null || m_ExecutingEvents == null)
            {
                throw new InvalidOperationException("BaseStart() must be called in Start()");
            }

            lock (QueueLock)
            {
                m_QueuedEvents.Add(action);
            }
        }

        #endregion

        #region Private methods

        private void HandleEvents()
        {
            MoveQueuedEventsToExecuting();

            while (m_ExecutingEvents.Count > 0)
            {
                Action e = m_ExecutingEvents[0];
                m_ExecutingEvents.RemoveAt(0);
                e();
            }
        }

        private void MoveQueuedEventsToExecuting()
        {
            lock (QueueLock)
            {
                while (m_QueuedEvents.Count > 0)
                {
                    Action e = m_QueuedEvents[0];
                    m_ExecutingEvents.Add(e);
                    m_QueuedEvents.RemoveAt(0);
                }
            }
        }

        #endregion
    }
}
