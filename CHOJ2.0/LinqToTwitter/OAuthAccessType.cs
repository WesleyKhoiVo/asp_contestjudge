using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToTwitter
{
    /// <summary>
    /// type of access requested
    /// </summary>
    public enum OAuthAccessType
    {
        /// <summary>
        /// read and write (Default), same as Twitter default
        /// </summary>
        ReadWrite,

        /// <summary>
        /// read only
        /// </summary>
        Read
    }
}
