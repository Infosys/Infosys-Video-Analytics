/****************************************************************
 * This file is a part of the Legacy Integration Framework.
 * This file contains the LegacyException definition.
 * Copyright (c) 2003 - 2005 Infosys Technologies Ltd. All Rights Reserved.
 ***************************************************************/

using System;
using System.Runtime.Serialization;

namespace Infosys.Lif.LegacyCommon
{
	/// <summary>
	/// Represent application exception thrown from legacy integration 
	/// framework.
	/// </summary>
	[Serializable]
	public sealed class LegacyException:Exception
	{
		#region Methods

		#region parameter less constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public LegacyException() : base() 
		{
		}
		#endregion

		#region one parameter constructor 
		/// <summary>
		/// Initializes with a specified error message. 
		/// </summary>
		/// <param name="message">A message that describes the error.</param>
		public LegacyException(string message) : base(message) 
		{
		}
		#endregion

		#region Two parameter constructor for setting inner exception.
		/// <summary>
		/// Initializes with a specified error 
		/// message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.
		/// </param>
		/// <param name="exception">The exception that is the cause of the current exception. 
		/// If the innerException parameter is not a null reference, the current exception 
		/// is raised in a catch block that handles the inner exception.
		/// </param>
		public LegacyException(string message, Exception exception) : 
			base(message, exception) 
		{
		}
		#endregion
		
		#region Two parameter constructor for setting SerializationInfo
		/// <summary>
		/// Initializes with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.
		/// </param>
		private LegacyException(SerializationInfo info, StreamingContext context)
			:base(info, context) 
		{
		}
		#endregion

		#endregion
	}
    [Serializable]
    public sealed class QueueNotRespondedException : Exception
    {
        #region Methods

        #region parameter less constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public QueueNotRespondedException() : base()
        {
        }
        #endregion

        #region one parameter constructor 
        /// <summary>
        /// Initializes with a specified error message. 
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public QueueNotRespondedException(string message) : base(message)
        {
        }
        #endregion

        #region Two parameter constructor for setting inner exception.
        /// <summary>
        /// Initializes with a specified error 
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.
        /// </param>
        /// <param name="exception">The exception that is the cause of the current exception. 
        /// If the innerException parameter is not a null reference, the current exception 
        /// is raised in a catch block that handles the inner exception.
        /// </param>
        public QueueNotRespondedException(string message, Exception exception) :
            base(message, exception)
        {
        }
        #endregion

        #region Two parameter constructor for setting SerializationInfo
        /// <summary>
        /// Initializes with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.
        /// </param>
        private QueueNotRespondedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #endregion
    }
}
