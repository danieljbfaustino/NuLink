﻿using System;
using NuLink.TestCase.FirstPackage;

namespace NuLink.TestCase.ConsumerLib
{
    public class ConsumerClassOne
    {
        public string ConsumeStringFromFirstPackage()
        {
            var first = new FirstClass();
            var firstString = first.GetString();
            return $"consumer-class-one:{firstString}";
        }
    }
}