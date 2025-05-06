/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity
{
    public class PostgressqlExtension
    {
        string connectionString = "Host=[Host];Database=[Database];Username=[Username];Password=[Password]";
      
      





        public dynamic ExecuteScalarCmd(string cmdstring, Dictionary<string, dynamic> parameters)
        {
            dynamic result;
            try
            {
                
                using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(cmdstring, conn))
                    {

                        foreach(var para in parameters)
                        {
                            cmd.Parameters.AddWithValue(para.Key, para.Value);
                        }
                       
                        result = cmd.ExecuteScalar();
                    }

                }
                return result;
             
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception in ExecuteNonQueryCmd , Exception message : {0} , stack trace : {1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                return false;
            }
        }

        
        public dynamic ExecuteNonQueryCmd(string cmdstring, Dictionary<string, dynamic> parameters)
        {
            dynamic result;
            try
            {
               
                using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(cmdstring, conn))
                    {
                        int i = 0;
                        
                        foreach (var para in parameters)
                        {
                            cmd.Parameters.AddWithValue(para.Key, para.Value);
                        }

                        result = cmd.ExecuteNonQuery();
                       
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception in ExecuteNonQueryCmd , Exception message : {0} , stack trace : {1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                return false;
            }
        }
      
        public DataTable ExecuteCmdWithDataAdapter(string cmdstring, Dictionary<string, dynamic> parameters)
        {
            dynamic result;
            
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
               
                using (NpgsqlCommand cmd = new NpgsqlCommand(cmdstring, conn))
                {
                    int i = 0;
                    
                    foreach (var para in parameters)
                    {
                        cmd.Parameters.AddWithValue(para.Key, para.Value);
                    }

                    
                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(cmd))
                    {
                        dataSet.Reset();
                        dataAdapter.Fill(dataSet);
                        dataTable = dataSet.Tables[0];
                    }
                       
                }

            }

            return dataTable;
        }



       




        


    }
}
