/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using System;
using System.Runtime.Serialization;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public class FaceMaskDetectionException : System.Exception, ISerializable
    {
        public FaceMaskDetectionException()
            : base()
        {

        }
        public FaceMaskDetectionException(string message)
                : base(message)
        {

        }
        public FaceMaskDetectionException(string message, Exception inner)
                : base(message, inner)
        {

        }
        protected FaceMaskDetectionException(SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
           
        }
    }

    [Serializable]
    public class FaceMaskDetectionCriticalException : System.Exception, ISerializable
    {
        public FaceMaskDetectionCriticalException() : base()
        {

        }
        public FaceMaskDetectionCriticalException(string message) : base(message)
        {

        }
        public FaceMaskDetectionCriticalException(string message, Exception inner) : base(message, inner)
        {

        }
        protected FaceMaskDetectionCriticalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }
    }

    [Serializable]
    public class FaceMaskDetectionDataItemNotFoundException : System.Exception, ISerializable
    {
        public FaceMaskDetectionDataItemNotFoundException() : base()
        {

        }
        public FaceMaskDetectionDataItemNotFoundException(string message) : base(message)
        {

        }
        public FaceMaskDetectionDataItemNotFoundException(string message, Exception inner) : base(message, inner)
        {

        }
        protected FaceMaskDetectionDataItemNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
          
        }

    }

  
    [Serializable]
    public class FaceMaskDetectionValidationException : System.Exception, ISerializable
    {
        public FaceMaskDetectionValidationException()
            : base()
        {

        }
        public FaceMaskDetectionValidationException(string message)
            : base(message)
        {

        }
        public FaceMaskDetectionValidationException(string message, Exception inner)
            : base(message, inner)
        {

        }
        protected FaceMaskDetectionValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

    }
   
    [Serializable]
    public class FaceMaskDetectionInvalidConfigException : System.Exception, ISerializable
    {
        public FaceMaskDetectionInvalidConfigException()
            : base()
        {

        }
        public FaceMaskDetectionInvalidConfigException(string message)
            : base(message)
        {

        }
        public FaceMaskDetectionInvalidConfigException(string message, Exception inner)
            : base(message, inner)
        {

        }
        protected FaceMaskDetectionInvalidConfigException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

    }
  
    [Serializable]
    public class FaceMaskDetectionFrameGrabberZipperException : System.Exception, ISerializable
    {
        public FaceMaskDetectionFrameGrabberZipperException()
            : base()
        {

        }
        public FaceMaskDetectionFrameGrabberZipperException(string message)
            : base(message)
        {

        }
        public FaceMaskDetectionFrameGrabberZipperException(string message, Exception inner)
            : base(message, inner)
        {

        }
        protected FaceMaskDetectionFrameGrabberZipperException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

    }

    [Serializable]
    public class FaceMaskDetectionVideoCompletedException : System.Exception, ISerializable
    {
        public FaceMaskDetectionVideoCompletedException()
            : base()
        {

        }
        public FaceMaskDetectionVideoCompletedException(string message)
            : base(message)
        {

        }
        public FaceMaskDetectionVideoCompletedException(string message, Exception inner)
            : base(message, inner)
        {

        }
        protected FaceMaskDetectionVideoCompletedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
           
        }

    }




    [Serializable]
    public class ClientInactiveException : Exception
    {
        public ClientInactiveException()
        {

        }

        public ClientInactiveException(string message)
            : base(message)
        {

        }


        protected ClientInactiveException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
          
        }

    }

    public class ClientNotConnectedException : Exception
    {
        public ClientNotConnectedException()
        {

        }

        public ClientNotConnectedException(string message)
            : base(message)
        {

        }


        protected ClientNotConnectedException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
      
        }

    }


    public class ClientDisconnectedException : Exception
    {
        public ClientDisconnectedException()
        {

        }

        public ClientDisconnectedException(string message)
            : base(message)
        {

        }


        protected ClientDisconnectedException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
          
        }

    }

    [Serializable]
    public class DuplicateRecordException : Exception
    {
        public DuplicateRecordException()
        {

        }

        public DuplicateRecordException(string message)
            : base(message)
        {

        }


        protected DuplicateRecordException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
         
        }

    }

    [Serializable]
    public class TaskRouteNotFoundException : Exception
    {
        public TaskRouteNotFoundException()
        {



        }



        public TaskRouteNotFoundException(string message)
            : base(message)
        {



        }




        protected TaskRouteNotFoundException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
           
        }



    }

}

