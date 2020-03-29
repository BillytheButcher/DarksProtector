﻿using System;

namespace DarksProtector
{
    public class StringItem : IViewModel<string>
    {
        public StringItem(string item)
        {
            Item = item;
        }

        public string Item { get; private set; }

        string IViewModel<string>.Model
        {
            get { return Item; }
        }

        public override string ToString()
        {
            return Item;
        }
    }
}