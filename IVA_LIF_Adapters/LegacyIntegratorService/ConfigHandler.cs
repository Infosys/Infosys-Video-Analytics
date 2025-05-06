/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;


namespace Infosys.Lif.LegacyIntegratorService
{
    internal class ConfigHandler : ConfigurationSection
    {

       
        protected override object GetRuntimeObject()
        {
            return configurations;
        }
        static LISettings configurations;

        public LISettings Configurations
        {
            get { return configurations; }
        }

        protected override void DeserializeElement(XmlReader reader,
            bool serializeCollectionKey)
        {
            XmlSerializer serializer
                = new XmlSerializer(typeof(LISettings));
            configurations = (LISettings)serializer.Deserialize(reader);
        }

        protected override bool SerializeElement(XmlWriter writer,
            bool serializeCollectionKey)
        {

            if (writer == null)
            {
                return true;
            }
            XmlSerializer serializer
                = new XmlSerializer(typeof(LISettings));
            serializer.Serialize(writer, configurations);
            return true;
        }

    }
}
