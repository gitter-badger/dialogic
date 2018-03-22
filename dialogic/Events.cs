﻿using System;
using System.Collections.Generic;

namespace Dialogic
{
    /// /////////////////////////////////////////////////////////

    /**
     * Tells Dialogic that the User has made a specific choice in response
     * to a prompt
     */
    public interface IChoice
    {
        int GetChoiceIndex();
    }

    /**
     * Tells Dialogic to restart the current Chat after a indefinite WAIT cmd, 
     * or an ISuspend event, optionally with a specific chat, specified with its 
     * Label, or with a Find, specified via its metadata constraints.
     */
    public interface IResume
    {
        string ResumeWith();
    }

    /**
     * Suspends the current Chat. The current chat (or a new chat) can be resumed by 
     * sending a ResumeEvent.
     */
    public interface ISuspend { }

    /**
     * Tells Dialogic to clear its stack of past chats, leaving none to be resumed
     */
    public interface IClear  { }

    /**
     * Superclass for specific GameEvents
     */
    public abstract class GameEvent : EventArgs { }


    /**
     * Send by Dialogic whenever an IEmittable Command (e.g., Say, Ask, Do, Wait) 
     * is invoked, so that it may be handled by the application
     */
    public interface IUpdateEvent
    {
        string Text();
        string Type();
        string Get(string name, string def = null);
        void RemoveKeys(params string[] keys);
        IDictionary<string, object> Data();

        int GetInt(string name, int def = -1);
        bool GetBool(string key, bool def = false);
        double GetDouble(string key, double def = -1);
        float GetFloat(string key, float def = -1);
    }


    // ---------------------- Implementations -----------------------


    /**
     * Basic implementation of ISuspend
     */
    public class SuspendEvent : GameEvent, ISuspend { }


    /**
     * Basic implementation of IClear
     */
    public class ClearEvent : GameEvent, IClear { }

    /**
     * Basic implementation of IResume
     */
    public class ResumeEvent : GameEvent, IResume
    {
        public readonly string data;

        public ResumeEvent(string chatLabel = null) : base()
        {
            this.data = chatLabel;
        }

        public string ResumeWith()
        {
            return data;
        }
    }

    /**
     * * Basic implementation of IChoice
     */
    public class ChoiceEvent : GameEvent, IChoice
    {
        protected int choiceIndex;

        public ChoiceEvent(int option)
        {
            this.choiceIndex = option;
        }

        public override string ToString()
        {
            return "Choice: " + choiceIndex;
        }

        public int GetChoiceIndex()
        {
            return choiceIndex;
        }
    }

    /**
     * Basic implementation of IUpdateEvent which wraps a string/object 
     * dictionary containing relevant Command data with helper functions
     * for extracting specific types. 
     */
    public class UpdateEvent : IUpdateEvent
    {
        private IDictionary<string, object> data;

        public UpdateEvent(Command c)
        {
            this.data = c.realized;
        }

        public UpdateEvent(IDictionary<string, object> data)
        {
            this.data = data;
        }

        public IDictionary<string, object> Data()
        {
            return data;
        }

        public string Get(string key, string def = null)
        {
            return data.ContainsKey(key) ? (string)data[key] : def;
        }

        public int GetInt(string key, int def = -1)
        {
            string s = Get(key, def + String.Empty);
            return (int)Convert.ChangeType(s, typeof(int));
        }

        public bool GetBool(string key, bool def = false)
        {
            string s = Get(key, def + String.Empty);
            return (bool)Convert.ChangeType(s, typeof(bool));
        }

        public float GetFloat(string key, float def = -1)
        {
            string s = Get(key, def + String.Empty);
            return (float)Convert.ChangeType(s, typeof(float));
        }

        public double GetDouble(string key, double def = -1)
        {
            string s = Get(key, def + String.Empty);
            return (double)Convert.ChangeType(s, typeof(double));
        }

        public void RemoveKeys(params string[] keys)
        {
            foreach (var k in keys) data.Remove(k);
        }

        public string Text()
        {
            return (string)data[Meta.TEXT];
        }

        public string Type()
        {
            return (string)data[Meta.TYPE];
        }
    }

}