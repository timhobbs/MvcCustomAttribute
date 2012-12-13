using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using MvcCustomAttribute.Models;

namespace MvcCustomAttribute.DataAccess {

    /// <summary>
    /// Summary description for ShipRateLookup
    /// </summary>
    public class ShipRateLookup {

        public string ConnectionString {
            get {
                return ConfigurationManager.ConnectionStrings["data"].ConnectionString;
            }
        }

        public ShipRateLookup() {
        }

        public IList<SelectListItem> GetCarrierList() {
            IList<SelectListItem> carriers = new List<SelectListItem>();
            carriers.Add(new SelectListItem() { Text = "Select a carrier", Value = "", Selected = true });
            carriers.Add(new SelectListItem() { Text = "UPS Rates", Value = "UPS" });
            carriers.Add(new SelectListItem() { Text = "Postal Rates", Value = "POS" });
            return carriers;
        }

        public IList<SelectListItem> GetShipMethods(string carrier) {
            IList<SelectListItem> shipMethods = new List<SelectListItem>();
            shipMethods.Add(new SelectListItem() { Text = "Select a method", Value = "", Selected = true });
            switch (carrier) {
                case "POS":
                    shipMethods.Add(new SelectListItem() { Text = "Media Mail", Value = "MM" });
                    break;

                default:
                    shipMethods.Add(new SelectListItem() { Text = "Ground", Value = "Ground" });
                    shipMethods.Add(new SelectListItem() { Text = "3 Day Select", Value = "3day" });
                    shipMethods.Add(new SelectListItem() { Text = "2nd Day Air", Value = "2day" });
                    shipMethods.Add(new SelectListItem() { Text = "Next Day Air", Value = "Nextdayam" });
                    break;
            }

            return shipMethods;
        }

        public IList<ShipRateDetail> GetUpsRates(string method, List<int> weights, string zip) {
            string weight = String.Empty;
            if (weights.Count == 1) {
                weight = String.Format(" = {0}", weights[0]);
            } else {
                foreach (var w in weights) {
                    if (weight != String.Empty) weight += ", ";
                    weight += String.Format(" '{0}' ", w);
                }

                weight = String.Format(" IN ( {0} ) ", weight);
            }

            string sql = String.Format(@"SELECT rates.cost AS TotalCost, rates.lbs AS Weight
                                        FROM rates
                                        INNER JOIN zones ON zones.[{2}] = rates.zone_id
                                        WHERE zones.zip = '{0}'
                                        AND rates.lbs {1}", zip.Substring(0, 3), weight, method);
            return ConvertTo<ShipRateDetail>(DataAccess.GetData(this.ConnectionString, sql)).ToList();
        }

        public ShipRateDetail GetPosRates(int weight) {
            string sql = String.Format(@"SELECT mediamail.cost AS TotalCost
                                        FROM mediamail
                                        WHERE mediamail.weight = {0}", weight);
            IList<ShipRateDetail> rates = ConvertTo<ShipRateDetail>(DataAccess.GetData(this.ConnectionString, sql));
            if (rates.Count == 0) return null;

            return rates[0];
        }

        /// <summary>
        /// From http://lozanotek.com/blog/archive/2007/05/09/Converting_Custom_Collections_To_and_From_DataTable.aspx
        /// Includes the fix for nullable types in the comments:
        /// http://lozanotek.com/blog/archive/2007/05/09/Converting_Custom_Collections_To_and_From_DataTable.aspx#26421
        /// </summary>

        /// <summary>
        /// Converts a list of typed objects to a DataTable.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="list">List of objects</param>
        /// <returns>DataTable</returns>
        public static DataTable ConvertTo<T>(IList<T> list) {
            DataTable table = CreateTable<T>();
            Type entityType = typeof(T);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

            foreach (T item in list) {
                DataRow row = table.NewRow();

                foreach (PropertyDescriptor prop in properties) {
                    row[prop.Name] = prop.GetValue(item);
                }

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Converts a list of DataRows to a typed list.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="rows">DataRow list</param>
        /// <returns>A list of typed objects</returns>
        public static IList<T> ConvertTo<T>(IList<DataRow> rows) {
            IList<T> list = null;

            if (rows != null) {
                list = new List<T>();

                foreach (DataRow row in rows) {
                    T item = CreateItem<T>(row);
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Converts a DataTable to a typed list.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>A list of typed objects</returns>
        public static IList<T> ConvertTo<T>(DataTable table) {
            if (table == null) {
                return null;
            }

            List<DataRow> rows = new List<DataRow>();

            foreach (DataRow row in table.Rows) {
                rows.Add(row);
            }

            return ConvertTo<T>(rows);
        }

        /// <summary>
        /// Converts a DataRow to an object.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="row">DataRow</param>
        /// <returns>Typed object</returns>
        public static T CreateItem<T>(DataRow row) {
            T obj = default(T);
            if (row != null) {
                obj = Activator.CreateInstance<T>();

                foreach (DataColumn column in row.Table.Columns) {
                    PropertyInfo prop = obj.GetType().GetProperty(column.ColumnName);
                    try {
                        object value = row[column.ColumnName];
                        if (value.GetType() == typeof(System.DBNull)) {

                            // This prevents the dreaded "Object of type 'System.DBNull' cannot be converted
                            // to type 'System.String'" error.
                            value = null;
                        }

                        prop.SetValue(obj, value, null);
                    } catch {

                        // You can log something here
                        throw;
                    }
                }
            }

            return obj;
        }

        /// <summary>
        /// Creates a DataTable of typed objects.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <returns>DataTable</returns>
        public static DataTable CreateTable<T>() {
            Type entityType = typeof(T);
            DataTable table = new DataTable(entityType.Name);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

            foreach (PropertyDescriptor prop in properties) {
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    NullableConverter converter = new NullableConverter(prop.PropertyType);
                    table.Columns.Add(prop.Name, converter.UnderlyingType);
                } else {
                    table.Columns.Add(prop.Name, prop.PropertyType);
                }
            }

            return table;
        }
    }
}