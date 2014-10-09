﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spidermonkey;

namespace Tests {
    [TestFixture]
    public class Tests {
        public static /* readonly */ JSClass SandboxClass = new JSClass {
        };

        public static void ErrorReporter (JSHandleContext context, string message, ref JSErrorReport report) {
            throw new Exception();
        }

        [TestCase]
        public void CanInitContext () {
            // Assert.IsTrue(JSAPI.Init());

            var runtime = JSAPI.NewRuntime(1024 * 1024);
            Assert.AreNotEqual(IntPtr.Zero, runtime);

            var context = JSAPI.NewContext(runtime, 8192);
            Assert.AreNotEqual(IntPtr.Zero, context);

            JSErrorReporter errorReporter = ErrorReporter;

            // SetErrorReporter returns previously-active reporter
            Assert.AreEqual(null, JSAPI.SetErrorReporter(context, errorReporter));
            Assert.AreEqual(errorReporter, JSAPI.SetErrorReporter(context, errorReporter));

            var global = new JSRootedObject(context, JSAPI.NewGlobalObject(
                context, 
                ref JSClass.DefaultGlobalObjectClass, 
                null,
                JSOnNewGlobalHookOption.DontFireOnNewGlobalHook,
                ref JSCompartmentOptions.Default
            ));
            Assert.AreNotEqual(IntPtr.Zero, global);

            JSAPI.InitStandardClasses(context, global);
        }
    }
}
