/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Elasticsearch.Net;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Index;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.IDataAccess;
using Microsoft.Extensions.Caching.Memory;
using Nest;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess.ElasticSearch
{
    public class FrameMetaDataActionDS : IEntity<FrameMetaData>
    {
         
        static string index = ApplicationConstants.IndexDetails.FrameMetaDataActionStagingIndex;
        ElasticsearchExtensions elasticsearchExtensions = new ElasticsearchExtensions(index);
        public bool Delete(FrameMetaData entity)
        {
            throw new NotImplementedException();
        }

        public IList<FrameMetaData> GetAll()
        {
            throw new NotImplementedException();
        }
        public IList<FrameMetaData> GetByType(FrameMetaDataEntity fs,string index)
        {
            var frameMetaDataActions=new List<FrameMetaData>();
            var client = elasticsearchExtensions.client;
            try
            {
                var searchResponse = client.Search<FrameMetaData>(s => s.Index(index)
        .Query(q => q
            .Bool(b => b
                .Must(
                mu => mu.Match(m => m
                                     .Field(f => f.Tid)
                                     .Query(Convert.ToString(fs.Tid))
                                         ),
                mu => mu.Match(m => m
                        .Field(f => f.Did)
                        .Query(fs.Did)
                    ),
                mu => mu.Match(m => m
                        .Field(f => f.Fid)
                        .Query(Convert.ToString(fs.Fid))
                    ),
                mu => mu.Match(m => m
                        .Field(f => f.PredictionType)
                        .Query(fs.PredictionType)
                    ),
                mu => mu
                    .Bool(bb => bb

                        .Filter(sh => sh
                            .DateRange(r => r
                   .Field(f => f.CreatedDate)
                   .GreaterThanOrEquals(fs.StartTime)
                   .LessThan(fs.EndTime)
               )

                    )
                    )
                )
            )
        )
    );


                frameMetaDataActions = searchResponse.Documents.ToList();
                
                
            }
            catch (Exception ex)
            {
                LogHandler.LogError("error during search operation : message {0}", LogHandler.Layer.Business, ex.Message);
                throw;
            }
            return frameMetaDataActions;
        }
        public string GetLabelStatus(Predictions[] BEPredArr, string fsname, string index)
        {
            
            var frameMetaDataActions = new List<FrameMetaData>();
            List<string> esdata = new List<string>();
            var client = elasticsearchExtensions.client;

            

            bool chkrvalue = BEPredArr[0].Lb.Equals(fsname);
            bool value = false;
            string rval = "";
            try
            {
                if (chkrvalue == true)
                {
                    Console.WriteLine("Passed First Condition");
                    var milliseconds = 3000;
                    Thread.Sleep(milliseconds);
                    client.Indices.Refresh();
                    var searchResponse = client.Search<FrameMetaData>(s => s
                                                    .Index(index)
                                                    .Query(q => q.MatchAll()
                                                           )
                                                     );
                    frameMetaDataActions = searchResponse.Documents.ToList();
                    Console.WriteLine(frameMetaDataActions.Count);
                    int i = 0;
                    int j = 0;
                    if (frameMetaDataActions.Count > 0)
                    {
                        for (i = 0; i < frameMetaDataActions.Count; i++)
                        {
                            int fscount = frameMetaDataActions[i].Fs.Count();
                            for (j = 0; j < fscount; j++)
                            {
                                esdata.Add(frameMetaDataActions[i].Fs[j].Lb);
                                Console.WriteLine("Value Added in List {0}", frameMetaDataActions[i].Fs[j].Lb);
                            }
                        }

                        
                        Console.WriteLine("Check Label Exists or Not");
                        value = esdata.Contains(fsname);
                        if (value == true)
                        {
                            rval = "Exists";
                        }
                        else
                        {
                            rval = "Not Exists";
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("Inserting First Record");
                        rval = "Not Exists";
                    }
                }
                else
                {
                    rval = "Label Not Exists";
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }
            return rval;
        }

        public IList<FrameMetaData> GetUnprocessedFlag()
        {
            var customerActions = new List<FrameMetaData>();
            
            return customerActions;
        }

        public IList<FrameMetaData> GetAll(FrameMetaData Entity)
        {
            throw new NotImplementedException();
        }



        public IQueryable<FrameMetaData> GetAny()
        {
            throw new NotImplementedException();
        }

        public FrameMetaData GetOne(FrameMetaData Entity)
        {
            throw new NotImplementedException();
        }



        public bool Insert(FrameMetaData entity,string index)
        {
            var client = elasticsearchExtensions.client;

            try
            {
                
                if (!client.Indices.Exists(index).Exists)
                {
                    
                    var res = client.Indices.Create(index, c => c
                    .Map<FrameMetaData>(m => m.AutoMap()));
                    
                    if (!res.IsValid)
                    {

                        
                        return false;
                    }


                }
                var response = elasticsearchExtensions.client.Index(entity, i => i
                    .Index(index));

                client.Indices.Refresh(index);
                if (!response.IsValid)
                {
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError("error during insert operation : message {0}", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }
           
            return true;
        }

        public IList<FrameMetaData> InsertBatch(IList<FrameMetaData> entities)
        {
            throw new NotImplementedException();
        }

        public bool UpdateCustomerActionssValue(FrameMetaData entity)
        {
            var result = false;
           
            return result;
        }


        public bool UpdateCustomerActions(FrameMetaData entity)
        {
            throw new NotImplementedException();
        }


        public string GetCustomerActionssValue(FrameMetaData entity)
        {
            throw new NotImplementedException();

        }
        public FrameMetaData Update(FrameMetaData entity)
        {
            throw new NotImplementedException();
        }

        public IList<FrameMetaData> UpdateBatch(IList<FrameMetaData> entities)
        {
            var result = false;
            
            return result ? entities : null;
        }

        FrameMetaData  IEntity<FrameMetaData>.Insert(FrameMetaData entity)
        {
            throw new NotImplementedException();
        }
    }
}
