// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbecký" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

namespace Debugger.Wrappers.CorDebug
{
	using System;
	
	
	public class ISequentialStream
	{
		
		private Debugger.Interop.CorDebug.ISequentialStream wrappedObject;
		
		internal Debugger.Interop.CorDebug.ISequentialStream WrappedObject
		{
			get
			{
				return this.wrappedObject;
			}
		}
		
		public ISequentialStream(Debugger.Interop.CorDebug.ISequentialStream wrappedObject)
		{
			this.wrappedObject = wrappedObject;
		}
		
		public static ISequentialStream Wrap(Debugger.Interop.CorDebug.ISequentialStream objectToWrap)
		{
			return new ISequentialStream(objectToWrap);
		}
		
		public bool Is<T>() where T: class
		{
			try {
				CastTo<T>();
				return true;
			} catch {
				return false;
			}
		}
		
		public T As<T>() where T: class
		{
			try {
				return CastTo<T>();
			} catch {
				return null;
			}
		}
		
		public T CastTo<T>() where T: class
		{
			return (T)Activator.CreateInstance(typeof(T), this.WrappedObject);
		}
		
		public static bool operator ==(ISequentialStream o1, ISequentialStream o2)
		{
			return ((object)o1 == null && (object)o2 == null) ||
			       ((object)o1 != null && (object)o2 != null && o1.WrappedObject == o2.WrappedObject);
		}
		
		public static bool operator !=(ISequentialStream o1, ISequentialStream o2)
		{
			return !(o1 == o2);
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public override bool Equals(object o)
		{
			ISequentialStream casted = o as ISequentialStream;
			return (casted != null) && (casted.WrappedObject == wrappedObject);
		}
		
		
		public void RemoteRead(out byte pv, uint cb, out uint pcbRead)
		{
			this.WrappedObject.RemoteRead(out pv, cb, out pcbRead);
		}
		
		public void RemoteWrite(ref byte pv, uint cb, out uint pcbWritten)
		{
			this.WrappedObject.RemoteWrite(ref pv, cb, out pcbWritten);
		}
	}
}
