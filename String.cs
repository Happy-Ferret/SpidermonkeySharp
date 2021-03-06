﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spidermonkey.Managed {
    public class JSString : IDisposable {
        public readonly JSContextPtr Context;
        public readonly Rooted<JSStringPtr> Root;

        private Rooted<JS.Value> LazyRootedValue;

        /// <summary>
        /// Wraps existing reference and roots it.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="str"></param>
        public JSString (
            JSContextPtr context,
            JSStringPtr str
        ) {
            if (context.IsZero)
                throw new ArgumentNullException("context");

            Context = context;
            Root = new Rooted<JSStringPtr>(Context, str);
        }

        public JSString (Rooted<JSStringPtr> strRoot)
            : this(strRoot.Context, strRoot.Value) {
        }

        /// <summary>
        /// If valueRoot does not contain a string value, this will throw.
        /// </summary>
        public JSString (Rooted<JS.Value> valueRoot)
            : this(valueRoot.Context, valueRoot.Value.AsString) {
        }

        /// <summary>
        /// Creates a JS string by copying the characters from text.
        /// </summary>
        public JSString (JSContextPtr context, string text)
            : this(context, JSAPI.NewStringCopy(context, text)) {
        }

        /// <summary>
        /// Creates a JSString wrapper from a JS string pointer.
        /// If the pointer is zero, null is returned.
        /// </summary>
        public static JSString New (JSContextPtr context, JSStringPtr str) {
            if (str.IsZero)
                return null;

            return new JSString(context, str);
        }

        /// <summary>
        /// Creates a JSString wrapper from a JS value containing a string.
        /// If the value is null or undefined, null is returned.
        /// </summary>
        public static JSString New (JSContextPtr context, JS.Value str) {
            if (str.IsNullOrUndefined)
                return null;

            return new JSString(context, str.AsString);
        }

        public uint Length {
            get {
                return JSAPI.GetStringLength(Pointer);
            }
        }

        public JSStringPtr Pointer {
            get {
                return Root.Value;
            }
        }

        public void Dispose () {
            Root.Dispose();
        }

        // Since we're rooted it's safe to implicitly convert to a value
        public static implicit operator JS.Value (JSString self) {
            return new JS.Value(self.Root);
        }

        // We have to lazily construct a rooted JS.Value for ourselves so that
        //  it can serve as the address of our JS.Value in order to produce a JSHandleValue.
        // Gross.
        public static implicit operator JSHandleValue (JSString self) {
            if (self.LazyRootedValue == null)
                self.LazyRootedValue = new Rooted<JS.Value>(self.Context, self.Root);

            return self.LazyRootedValue;
        }

        public static implicit operator JSStringPtr (JSString self) {
            return self.Root.Value;
        }

        // FIXME: This causes an ambiguity with SetElement since it accepts either HandleValue or HandleString
        public static implicit operator JSHandleString (JSString self) {
            return self.Root;
        }

        public static explicit operator string (JSString self) {
            return self.ToString();
        }

        public override int GetHashCode () {
            return Pointer.GetHashCode();
        }

        // FIXME: Should this also compare the stringified values? Probably not desirable.
        public bool Equals (JSString rhs) {
            return Pointer.Equals(rhs.Pointer);
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSString;
            if (rhs != null)
                return Equals(rhs);
            else
                return base.Equals(obj);
        }

        public override string ToString () {
            return Root.Value.ToManagedString(Context);
        }
    }
}
