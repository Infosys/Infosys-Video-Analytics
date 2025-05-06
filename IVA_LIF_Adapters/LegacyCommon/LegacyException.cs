/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
/****************************************************************
 * This file is a part of the Legacy Integration Framework.
 * This file contains the LegacyException definition.
 * Copyright (c) 2003 - 2025 Infosys Technologies Ltd. All Rights Reserved.
 ***************************************************************/

using System;
using System.Runtime.Serialization;

namespace Infosys.Lif.LegacyCommon
{
	
	[Serializable]
	public sealed class LegacyException:Exception
	{
		#region Methods

		#region parameter less constructor
		
		public LegacyException() : base() 
		{
		}
		#endregion

		#region one parameter constructor 
		
		public LegacyException(string message) : base(message) 
		{
		}
		#endregion

		#region Two parameter constructor for setting inner exception.
		
		public LegacyException(string message, Exception exception) : 
			base(message, exception) 
		{
		}
		#endregion
		
		#region Two parameter constructor for setting SerializationInfo
		
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
      
        public QueueNotRespondedException() : base()
        {
        }
        #endregion

        #region one parameter constructor 
        
        public QueueNotRespondedException(string message) : base(message)
        {
        }
        #endregion

        #region Two parameter constructor for setting inner exception.
        
        public QueueNotRespondedException(string message, Exception exception) :
            base(message, exception)
        {
        }
        #endregion

        #region Two parameter constructor for setting SerializationInfo
       
        private QueueNotRespondedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #endregion
    }
}
