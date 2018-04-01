﻿using System;
using System.Collections.Generic;

namespace Dialogic
{
    public interface IActor
    {
        string Name();
        bool IsDefault();
        CommandDef[] Commands();
        Func<Command, bool> Validator();
    }

    public class Actor : IActor
    {
        public static IActor Default;

        readonly string name;
        readonly bool isDefault;
        readonly CommandDef[] commands;
        readonly Func<Command, bool> validator;

        public Actor(string label) : this(label, false, null, null) { }

        public Actor(string label, Func<Command, bool> validator = null, 
            params CommandDef[] commands) : this(label, false, validator, commands) { }

        public Actor(string label, bool isDefault = false, Func<Command, bool> 
            validator = null, params CommandDef[] commands)
        {
            this.name = label;
            this.isDefault = isDefault;
            this.validator = validator;
            this.commands = commands;
            if (isDefault) Default = this;
        }

        public string Name()
        {
            return name;
        }

        public bool IsDefault()
        {
            return this == Default;
        }

        public Func<Command, bool> Validator()
        {
            return validator;
        }

        public CommandDef[] Commands()
        {
            return commands;
        }
    }
}