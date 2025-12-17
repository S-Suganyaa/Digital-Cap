using Dapper;
using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Tank;
using DigitalCap.Core.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;

namespace DigitalCap.Persistence.Repositories
{
    public class TankRepository : RepositoryBase<VesselTank, Guid>, ITankRepository
    {

        public async Task<List<VesselTank>> GetTanks_VesselByIMO(string imonumber)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ImoNumber", imonumber);

                return (await Connection.QueryAsync<VesselTank>(
                    sql: "dbo.Get_Vessel_Tank_ByIMONumber",
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction)).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> PopulateTemplate(string imonumber = null, string vesseltype = null, string copyingimonumber = null)
        {
            try
            {
                var tanks = new List<VesselTank>();
                var gradings = new List<VesselTankGrading>();


                if (!string.IsNullOrEmpty(copyingimonumber))
                {
                    tanks = this.GetTanks_VesselByIMO(copyingimonumber).Result;


                    foreach (var tank in tanks)
                    {
                        tank.Id = Guid.NewGuid();
                        tank.ImoNumber = imonumber;
                        tank.CreatedDttm = DateTime.Now;
                        tank.UpdateDttm = DateTime.Now;
                        var result = this.CreateTank(tank).Result;
                    }

                }

                if (!string.IsNullOrEmpty(vesseltype))
                {
                    tanks = this.GetTanks_VesselByVesselType(vesseltype).Result;


                    foreach (var tank in tanks)
                    {
                        tank.Id = Guid.NewGuid();
                        tank.ImoNumber = imonumber;
                        tank.CreatedDttm = DateTime.Now;
                        tank.VesselType = null;
                        tank.UpdateDttm = DateTime.Now;
                        var result = this.CreateTank(tank).Result;
                    }

                }


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> PopulateTemplate(string imonumber = null, string vesseltype = null, string copyingimonumber = null, int projectid = 0, int copyingProjectId = 0)
        {
            try
            {
                var tanks = new List<VesselTank>();
                if (!string.IsNullOrEmpty(copyingimonumber))
                {
                    tanks = copyingProjectId == 0 ? this.GetTanks_VesselByIMO(copyingimonumber).Result : this.CreateTanks_VesselByProject(copyingimonumber, copyingProjectId, projectid, imonumber).Result;

                }
                if (!string.IsNullOrEmpty(vesseltype))
                {
                    tanks = this.GetTanks_VesselByVesselType(vesseltype).Result;
                    foreach (var tank in tanks)
                    {
                        tank.Id = Guid.NewGuid();
                        tank.ImoNumber = imonumber;
                        tank.CreatedDttm = DateTime.Now;
                        tank.VesselType = null;
                        tank.UpdateDttm = DateTime.Now;
                        tank.ProjectId = projectid;
                        var result = this.CreateTank(tank).Result;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<List<VesselTank>> GetTanks_VesselByVesselType(string vesseltype)
        {

            try
            {
                //using (var connection = new SqlConnection(azureDbConnString))
                //{

                return (await Connection.QueryAsync<VesselTank>(
                    sql: "dbo.Get_Vessel_Tank_ByVesselType",
                    param: new
                    {
                        VesselType = vesseltype
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction)).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<VesselTank>> GetTanks_VesselByDetails(string vesseltype, string imonumber)
        {
            try
            {
                return (await Connection.QueryAsync<VesselTank>(
                   sql: "dbo.Get_Vessel_Tank_ByDetails",
                   param: new
                   {
                       VesselType = vesseltype,
                       Imonumber = imonumber
                   },
                   commandType: CommandType.StoredProcedure,
                   transaction: Transaction)).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> CreateTank(VesselTank vesselTank)
        {
            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@VesselName", vesselTank.VesselName);
                parameters.Add("@VesselType", vesselTank.VesselType);
                parameters.Add("@TankName", vesselTank.TankName);
                parameters.Add("@Subheader", vesselTank.Subheader);
                parameters.Add("@TankTypeId", vesselTank.TankTypeId);
                parameters.Add("@ImoNumber", vesselTank.ImoNumber);
                parameters.Add("@IsActive", vesselTank.IsActive);
                parameters.Add("@IsDeleted", vesselTank.IsDeleted);
                parameters.Add("@CreatedDttm", vesselTank.CreatedDttm);
                parameters.Add("@UpdateDttm", vesselTank.UpdateDttm);
                parameters.Add("@TemplateId", vesselTank.TemplateId);
                parameters.Add("@RequiredInReport", vesselTank.RequiredInReport);
                parameters.Add("@ProjectId", vesselTank.ProjectId);

                await Connection.ExecuteAsync(
                            sql: "dbo.Create_Vessel_TankMapping",
                            param: parameters,
                            transaction: Transaction,
                            commandType: CommandType.StoredProcedure
                        );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vessel tank");
                return false;
            }
        }

        public async Task<List<ShipType>> GetShipType()
        {
            try
            {
                var result = await Connection.QueryAsync<ShipType>(
                    sql: "dbo.GetShipType",
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                );

                return result.ToList();

            }
            catch (Exception)
            {
                throw; // API layer can handle error
            }
        }

        public async Task<List<VesselTank>> GetTanks_VesselByProject(string imoNumber, int projectId)
        {
            try
            {
                var result = await Connection.QueryAsync<VesselTank>(
                      sql: "[dbo].[Get_Vessel_Tank_ByProject]",
                      param: new
                      {
                          ImoNumber = imoNumber,
                          ProjectId = projectId
                      },
                       transaction: Transaction,
                       commandType: CommandType.StoredProcedure
                  );

                return result.ToList();

            }
            catch
            {
                throw;
            }
        }       
    }
}
