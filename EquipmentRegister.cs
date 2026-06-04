using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Length_Stds_Environmental_Monitoring
{
    public class InventoryItem
    {
        public string Keywords { get; set; }
        public string Id { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Description { get; set; }
        public string Specifications { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string Loggable { get; set; }
        public string Traceable { get; set; }

    }

    public class CalibrationRecord
    {
        public string EquipmentId { get; set; }
        public string Quantity { get; set; }
        public int CalibrationIntervalYears { get; set; }
        public string ComponentName { get; set; }
        public string ReportId { get; set; }
        public DateTime? ReportIssueDate { get; set; }
        public DateTime? MeasurementStartDate { get; set; }
        public DateTime? MeasurementStopDate { get; set; }

        public CalibrationType CalibrationType { get; set; }
        
        public string EquationUnit { get; set; }

        public string Equation { get; set; }

    }


    public enum CalibrationType
    {
        Equation,
        Table,
        File,
        Unknown   // optional but very useful fallback
    }

    public class EquipmentRegister
    {

        private XDocument doc;
        private XNamespace msl = "";
        private XElement register;

        public EquipmentRegister()
        {
            doc = XDocument.Load(@"C:\Users\MSL Lab\Documents\GitHub\Length_Stds_Equipment_Register\register.xml");
            msl = "https://measurement.govt.nz/equipment-register";
            register = doc.Root;
        }

        /// <summary>
        /// General Inventory of all Equipment
        /// </summary>
        public List<InventoryItem> BasicInventory()
        {
            XElement register = doc.Root
                        ?? throw new InvalidOperationException("Invalid XML document");

            var items =
                from e in register.Elements(msl + "equipment")
                select new InventoryItem
                {
                    Keywords = (string)e.Attribute("keywords"),
                    Id = (string)e.Element(msl + "id"),
                    Manufacturer = (string)e.Element(msl + "manufacturer"),
                    Model = (string)e.Element(msl + "model"),
                    Serial = (string)e.Element(msl + "serial"),
                    Description = (string)e.Element(msl + "description"),
                    Location = (string)e.Element(msl + "location"),
                    Status = (string)e.Element(msl + "status"),
                    Loggable = (string)e.Element(msl + "loggable"),
                    Traceable = (string)e.Element(msl + "traceable")
                };
            return items.ToList();
        }
        /// <summary>
        /// General Inventory of Specified Equipment
        /// </summary>
        /// <param name="search_key">A keyword to find specific Equipment</param>
        public List<InventoryItem> WildCardInventory(string search_key)
        {
            XElement register = doc.Root
                        ?? throw new InvalidOperationException("Invalid XML document");


            var items =
                from e in register.Elements(msl + "equipment")
                let keywords = ((string)e.Attribute("keywords") ?? "")
                    .Split(' ')
                where keywords.Any(k =>
                    string.Equals(k, search_key, StringComparison.OrdinalIgnoreCase))
                select new InventoryItem
                {
                    Keywords = string.Join(" ", keywords),
                    Id = (string)e.Element(msl + "id"),
                    Manufacturer = (string)e.Element(msl + "manufacturer"),
                    Model = (string)e.Element(msl + "model"),
                    Serial = (string)e.Element(msl + "serial"),
                    Description = (string)e.Element(msl + "description"),
                    Location = (string)e.Element(msl + "location"),
                    Status = (string)e.Element(msl + "status"),
                    Loggable = (string)e.Element(msl + "loggable"),
                    Traceable = (string)e.Element(msl + "traceable")
                };

            return items.ToList();
        }

        /// <summary>
        /// Find whether a particular piece of equipment is loggable (e.g monitoring equipment)
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        public bool Loggable(string equipmentId)
        {
            XElement equipment =
                        doc.Root?
                           .Elements(msl + "equipment")
                           .FirstOrDefault(e =>
                               string.Equals(
                                   (string)e.Element(msl + "id"),
                                   equipmentId,
                                   StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    string.Format("Equipment with ID {0} was not found", equipmentId));

            // Per schema: default is false if element is missing or empty
            return (bool?)equipment.Element(msl + "loggable") ?? false;

        }

        /// <summary>
        /// Gets basic inventory details for a given piece of equipment
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        public InventoryItem EquipmentDetails(string equipmentId)
        {
            XElement equipment =
                        doc.Root?
                           .Elements(msl + "equipment")
                           .FirstOrDefault(e =>
                               string.Equals(
                                   (string)e.Element(msl + "id"),
                                   equipmentId,
                                   StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    string.Format("Equipment with ID {0} was not found", equipmentId));


            InventoryItem i = new InventoryItem
            {
                Keywords = (string)equipment.Attribute("keywords"),
                Id = (string)equipment.Element(msl + "id"),
                Manufacturer = (string)equipment.Element(msl + "manufacturer"),
                Model = (string)equipment.Element(msl + "model"),
                Serial = (string)equipment.Element(msl + "serial"),
                Description = (string)equipment.Element(msl + "description"),
                Location = (string)equipment.Element(msl + "location"),
                Status = (string)equipment.Element(msl + "status"),
                Loggable = (string)equipment.Element(msl + "loggable"),
                Traceable = (string)equipment.Element(msl + "traceable")
            };

            return i;

        }

        /// <summary>
        /// returns a list of unique laboratories listed in the register
        /// </summary>
        public List<string> GetUniqueEquipmentLocations()
        {
            return doc.Root?
                .Elements(msl + "equipment")
                .Select(e => (string)e.Element(msl + "location"))
                .Where(location => !string.IsNullOrWhiteSpace(location))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(location => location)
                .ToList()
                ?? new List<string>();
        }



        /// <summary>
        /// Gets a list of calibration records for a given piece of equipment.
        ///
        /// Each record represents a component/report combination.
        /// The type of calibration is determined using the CalibrationType enum.
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        /// <returns>List of CalibrationRecord objects</returns>
        public List<CalibrationRecord> CalData(string equipmentId)
        {
            XElement equipment =
                doc.Root?
                   .Elements(msl + "equipment")
                   .FirstOrDefault(e => string.Equals(
                       (string)e.Element(msl + "id"),
                       equipmentId,
                       StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    "Equipment '" + equipmentId + "' not found");

            var records =
                from measurand in equipment
                    .Element(msl + "calibrations")?
                    .Elements(msl + "measurand")
                    ?? Enumerable.Empty<XElement>()

                from component in measurand.Elements(msl + "component")

                from report in component.Elements()
                    .Where(el =>
                        el.Name == msl + "report" ||
                        el.Name == msl + "digitalReport")
                    .DefaultIfEmpty() // include components with no reports

                let isReport =
                    report != null && report.Name == msl + "report"

                let isDigitalReport =
                    report != null && report.Name == msl + "digitalReport"

                // ✅ Determine calibration type
                let calibrationType =
                    report == null
                        ? CalibrationType.Unknown
                        : report.Element(msl + "equation") != null
                            ? CalibrationType.Equation
                            : report.Element(msl + "table") != null
                                ? CalibrationType.Table
                                : (isDigitalReport || report.Element(msl + "file") != null)
                                    ? CalibrationType.File
                                    : CalibrationType.Unknown

                // ✅ Extract equation info if present
                let equationElement =
                    report != null ? report.Element(msl + "equation") : null

                let equationValue =
                    equationElement != null
                        ? (string)equationElement.Element(msl + "value")
                        : null

                let equationUnit =
                    equationElement != null
                        ? (string)equationElement.Element(msl + "unit")
                        : null

                select new CalibrationRecord
                {
                    EquipmentId = equipmentId,

                    Quantity =
                        (string)measurand.Attribute("quantity"),

                    CalibrationIntervalYears =
                        (int)measurand.Attribute("calibrationInterval"),

                    ComponentName =
                        (string)component.Attribute("name"),

                    ReportId =
                        (string)(report != null ? report.Attribute("id") : null),

                    // ✅ Only valid for <report>
                    ReportIssueDate =
                        isReport
                            ? (DateTime?)report.Element(msl + "reportIssueDate")
                            : null,

                    MeasurementStartDate =
                        isReport
                            ? (DateTime?)report.Element(msl + "measurementStartDate")
                            : null,

                    MeasurementStopDate =
                        isReport
                            ? (DateTime?)report.Element(msl + "measurementStopDate")
                            : null,

                    CalibrationType = calibrationType,

                    Equation = equationValue,
                    EquationUnit = equationUnit
                };

            return records.ToList();
        }


        /// <summary>
        /// Retrieves the latest calibration record for a given equipment ID
        /// and component name.
        ///
        /// "Latest" is defined as the calibration report (or digital report)
        /// with the most recent measurementStartDate.
        ///
        /// The function determines the calibration type using the following rules:
        ///   - Equation: if an <equation> element exists
        ///   - Table: if a <table> element exists
        ///   - File: if it is a <digitalReport> (or associated file)
        ///   - Unknown: if none of the above applies
        ///
        /// Parameters:
        ///   equipmentId    - The equipment ID (case-insensitive)
        ///   componentName  - The component name (case-insensitive)
        ///
        /// Returns:
        ///   A populated CalibrationRecord, or null if no calibration exists.
        /// </summary>
        public CalibrationRecord GetLatestCalibration(
            string equipmentId,
            string componentName)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentException("equipmentId must not be empty.");

            if (string.IsNullOrWhiteSpace(componentName))
                throw new ArgumentException("componentName must not be empty.");

            XNamespace msl = "https://measurement.govt.nz/equipment-register";

            // ✅ Locate the equipment element
            XElement equipment =
                doc.Root
                    .Elements(msl + "equipment")
                    .FirstOrDefault(e =>
                        string.Equals(
                            (string)e.Element(msl + "id"),
                            equipmentId,
                            StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    "Equipment '" + equipmentId + "' not found.");

            // ✅ Find all reports for the specified component and select the latest
            var latest =
                (from measurand in equipment
                     .Element(msl + "calibrations")?
                     .Elements(msl + "measurand")
                 ?? Enumerable.Empty<XElement>()

                 from component in measurand.Elements(msl + "component")

                     // Match component name (case-insensitive)
                 where string.Equals(
                     (string)component.Attribute("name"),
                     componentName,
                     StringComparison.OrdinalIgnoreCase)

                 from report in component.Elements()
                     .Where(r =>
                         r.Name == msl + "report" ||
                         r.Name == msl + "digitalReport")

                 let startDate =
                     (DateTime?)report.Element(msl + "measurementStartDate")

                 // Only consider valid calibrations
                 where startDate.HasValue

                 select new
                 {
                     Measurand = measurand,
                     Component = component,
                     Report = report,
                     StartDate = startDate.Value
                 })
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefault();

            if (latest == null)
                return null;

            XElement reportElement = latest.Report;

            bool isReport = reportElement.Name == msl + "report";
            bool isDigitalReport = reportElement.Name == msl + "digitalReport";

            // ✅ Determine calibration type
            CalibrationType calibrationType = CalibrationType.Unknown;

            if (reportElement.Element(msl + "equation") != null)
            {
                calibrationType = CalibrationType.Equation;
            }
            else if (reportElement.Element(msl + "table") != null)
            {
                calibrationType = CalibrationType.Table;
            }
            else if (isDigitalReport || reportElement.Element(msl + "file") != null)
            {
                calibrationType = CalibrationType.File;
            }

            // ✅ Extract equation if present
            XElement equationElement = reportElement.Element(msl + "equation");

            string equationValue = null;
            string equationUnit = null;

            if (equationElement != null)
            {
                equationValue =
                    (string)equationElement.Element(msl + "value");

                equationUnit =
                    (string)equationElement.Element(msl + "unit");
            }

            // ✅ Build and return the calibration record
            CalibrationRecord result = new CalibrationRecord
            {
                EquipmentId = equipmentId,

                Quantity =
                    (string)latest.Measurand.Attribute("quantity"),

                CalibrationIntervalYears =
                    (int)latest.Measurand.Attribute("calibrationInterval"),

                ComponentName =
                    (string)latest.Component.Attribute("name"),

                ReportId =
                    (string)reportElement.Attribute("id"),

                // Dates only exist for <report>
                ReportIssueDate =
                    isReport
                        ? (DateTime?)reportElement.Element(msl + "reportIssueDate")
                        : null,

                MeasurementStartDate =
                    isReport
                        ? (DateTime?)reportElement.Element(msl + "measurementStartDate")
                        : null,

                MeasurementStopDate =
                    isReport
                        ? (DateTime?)reportElement.Element(msl + "measurementStopDate")
                        : null,

                CalibrationType = calibrationType,

                Equation = equationValue,
                EquationUnit = equationUnit
            };

            return result;
        }

        public string[][] GetLatestCalibrationTable(string equipmentId, string componentName)
        {

            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentException("equipmentId must not be empty", nameof(equipmentId));

            if (string.IsNullOrWhiteSpace(componentName))
                throw new ArgumentException("componentName must not be empty", nameof(componentName));



            XElement equipment =
                        doc.Root?
                           .Elements(msl + "equipment")
                           .FirstOrDefault(e =>
                               string.Equals(
                                   (string)e.Element(msl + "id"),
                                   equipmentId,
                                   StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    $"Equipment '{equipmentId}' not found.");

            var candidates =
                from measurand in equipment
                    .Element(msl + "calibrations")?
                    .Elements(msl + "measurand")
                    ?? Enumerable.Empty<XElement>()

                from component in measurand.Elements(msl + "component")
                where string.Equals(
                    (string)component.Attribute("name"),
                    componentName,
                    StringComparison.OrdinalIgnoreCase)

                from report in component.Elements()
                    .Where(el =>
                        el.Name == msl + "report" ||
                        el.Name == msl + "digitalReport")

                let startDate =
                    (DateTime?)report.Element(msl + "measurementStartDate")

                let table =
                    report.Element(msl + "table")

                where startDate.HasValue && table != null

                select new
                {
                    StartDate = startDate.Value,
                    Table = table
                };

            var latest = candidates
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefault();

            if (latest == null)
                return null; // no table for this component

            XElement tableElement = latest.Table;

            // Helper: split comma-separated values safely
            string[] SplitCsvLine(string value) =>
                value.Split(',')
                     .Select(v => v.Trim())
                     .ToArray();

            var rows = new List<string[]>();

            // Row 0: <type>
            rows.Add(SplitCsvLine(
                (string)tableElement.Element(msl + "type")));

            // Row 1: <unit>
            rows.Add(SplitCsvLine(
                (string)tableElement.Element(msl + "unit")));

            // Row 2: <header>
            rows.Add(SplitCsvLine(
                (string)tableElement.Element(msl + "header")));

            // Rows 3+: <data>
            string dataText = (string)tableElement.Element(msl + "data");

            var dataRows =
                dataText
                    .Split(new[] { '\n', '\r' },
                           StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => SplitCsvLine(line));

            rows.AddRange(dataRows);

            return rows.ToArray();
        }

        public ICalibration GetLatestCalibrationTableObject(string equipmentId, string componentName)
        {

            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentException("equipmentId must not be empty", nameof(equipmentId));

            if (string.IsNullOrWhiteSpace(componentName))
                throw new ArgumentException("componentName must not be empty", nameof(componentName));



            XElement equipment =
                        doc.Root?
                           .Elements(msl + "equipment")
                           .FirstOrDefault(e =>
                               string.Equals(
                                   (string)e.Element(msl + "id"),
                                   equipmentId,
                                   StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    $"Equipment '{equipmentId}' not found.");

            var candidates =
                from measurand in equipment
                    .Element(msl + "calibrations")?
                    .Elements(msl + "measurand")
                    ?? Enumerable.Empty<XElement>()

                from component in measurand.Elements(msl + "component")
                where string.Equals(
                    (string)component.Attribute("name"),
                    componentName,
                    StringComparison.OrdinalIgnoreCase)

                from report in component.Elements()
                    .Where(el =>
                        el.Name == msl + "report" ||
                        el.Name == msl + "digitalReport")

                let startDate =
                    (DateTime?)report.Element(msl + "measurementStartDate")

                let table =
                    report.Element(msl + "table")

                where startDate.HasValue && table != null

                select new
                {
                    StartDate = startDate.Value,
                    Table = table
                };

            var latest = candidates
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefault();

            if (latest == null)
                return null; // no table for this component

            XElement tableElement = latest.Table;

            // Helper: split comma-separated values safely
            string[] SplitCsvLine(string value) =>
                value.Split(',')
                     .Select(v => v.Trim())
                     .ToArray();

            var rows = new List<string[]>();

            // Row 0: <type>
            rows.Add(SplitCsvLine(
                (string)tableElement.Element(msl + "type")));

            // Row 1: <unit>
            rows.Add(SplitCsvLine(
                (string)tableElement.Element(msl + "unit")));

            // Row 2: <header>
            rows.Add(SplitCsvLine(
                (string)tableElement.Element(msl + "header")));

            // Rows 3+: <data>
            string dataText = (string)tableElement.Element(msl + "data");

            var dataRows =
                dataText
                    .Split(new[] { '\n', '\r' },
                           StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => SplitCsvLine(line));

            rows.AddRange(dataRows);

            string[][] array = rows.ToArray();
            ICalibration table_calibration = new TableCalibration(array);
            foreach (string col in array[2])
            {
                if(col.Contains("Rising") || col.Contains("Falling"))
                {
                    //this table is a hysteresis table, so we can return a HysteresisTable object instead of a Table object
                    table_calibration = new HysterisisTableCalibration(array);
                    return table_calibration;
                }
            }
            return table_calibration;
        }

        /// <summary>
        /// Gets the equation for a given component
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        /// <param name="componentName">The name of the component e.g Hygrometer</param>
        public string GetLatestEquationValue(string equipmentId, string componentName)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentException("equipmentId must not be empty", nameof(equipmentId));

            if (string.IsNullOrWhiteSpace(componentName))
                throw new ArgumentException("componentName must not be empty", nameof(componentName));

            XElement equipment =
                doc.Root?
                   .Elements(msl + "equipment")
                   .FirstOrDefault(e =>
                       string.Equals(
                           (string)e.Element(msl + "id"),
                           equipmentId,
                           StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    $"Equipment '{equipmentId}' not found.");

            var candidates =
                from measurand in equipment
                    .Element(msl + "calibrations")?
                    .Elements(msl + "measurand")
                    ?? Enumerable.Empty<XElement>()

                from component in measurand.Elements(msl + "component")
                where string.Equals(
                    (string)component.Attribute("name"),
                    componentName,
                    StringComparison.OrdinalIgnoreCase)

                from report in component.Elements()
                    .Where(el =>
                        el.Name == msl + "report" ||
                        el.Name == msl + "digitalReport")

                let startDate =
                    (DateTime?)report.Element(msl + "measurementStartDate")

                let equation =
                    report.Element(msl + "equation")

                where startDate.HasValue && equation != null

                select new
                {
                    MeasurementStartDate = startDate.Value,
                    EquationValue =
                        (string)equation.Element(msl + "value")
                };

            var latest = candidates
                .OrderByDescending(c => c.MeasurementStartDate)
                .FirstOrDefault();

            return latest?.EquationValue;
        }

        /// <summary>
        /// Gets the equation for a given component
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        /// <param name="componentNameFragment">A partial match of the component e.g Hygro</param>
        public string GetLatestEquationValuePartialMatch(string equipmentId, string componentNameFragment)
        {


            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentException("equipmentId must not be empty", nameof(equipmentId));

            if (string.IsNullOrWhiteSpace(componentNameFragment))
                throw new ArgumentException("componentNameFragment must not be empty", nameof(componentNameFragment));

            XElement equipment =
                doc.Root?
                   .Elements(msl + "equipment")
                   .FirstOrDefault(e =>
                       string.Equals(
                           (string)e.Element(msl + "id"),
                           equipmentId,
                           StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    string.Format("Equipment with ID {0} was not found", equipmentId));

            var candidates =
                from measurand in equipment
                    .Element(msl + "calibrations")?
                    .Elements(msl + "measurand")
                    ?? Enumerable.Empty<XElement>()

                from component in measurand.Elements(msl + "component")
                where ((string)component.Attribute("name"))?
                    .IndexOf(componentNameFragment,
                             StringComparison.OrdinalIgnoreCase) >= 0

                from report in component.Elements()
                    .Where(el =>
                        el.Name == msl + "report" ||
                        el.Name == msl + "digitalReport")

                let startDate =
                    (DateTime?)report.Element(msl + "measurementStartDate")

                let equationValue =
                    (string)report
                        .Element(msl + "equation")
                        ?.Element(msl + "value")

                where startDate.HasValue && !string.IsNullOrWhiteSpace(equationValue)

                select new
                {
                    MeasurementStartDate = startDate.Value,
                    Equation = equationValue.Trim()
                };

            return candidates
                .OrderByDescending(c => c.MeasurementStartDate)
                .Select(c => c.Equation)
                .FirstOrDefault();
        }




        /// <summary>
        /// Gets the address if it exists
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        /// <param name="addressType">can be "ip", "gpib" or "com"</param>
        public string Address(string equipmentId, string addressType)
        {

            XElement equipment =
                        doc.Root?
                           .Elements(msl + "equipment")
                           .FirstOrDefault(e =>
                               string.Equals(
                                   (string)e.Element(msl + "id"),
                                   equipmentId,
                                   StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    $"Equipment '{equipmentId}' was not found.");

            return equipment
                .Element(msl + "specifications")
                ?.Elements(msl + "addr")
                .FirstOrDefault(a =>
                    string.Equals(
                        (string)a.Attribute("type"),
                        addressType,
                        StringComparison.OrdinalIgnoreCase))
                ?.Value
                ?.Trim();


        }

        /// <summary>
        /// Gets the address if it exists
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        /// <param name="accessType">can be "ip", "gpib" or "com"</param>
        public string Access(string equipmentId, string accessType)
        {

            XElement equipment =
                        doc.Root?
                           .Elements(msl + "equipment")
                           .FirstOrDefault(e =>
                               string.Equals(
                                   (string)e.Element(msl + "id"),
                                   equipmentId,
                                   StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    $"Equipment '{equipmentId}' was not found.");

            return equipment
                .Element(msl + "specifications")
                ?.Elements(msl + "access")
                .FirstOrDefault(a =>
                    string.Equals(
                        (string)a.Attribute("type"),
                        accessType,
                        StringComparison.OrdinalIgnoreCase))
                ?.Value
                ?.Trim();


        }

        /// <summary>
        /// Gets the SICL interface ID for a network gateway
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        /// <param name="specificationElementName">The name of the specification element</param>
        public string SpecificationElement(string equipmentId, string specificationElementName)
        {
            XElement equipment =
                        doc.Root?
                           .Elements(msl + "equipment")
                           .FirstOrDefault(e =>
                               string.Equals(
                                   (string)e.Element(msl + "id"),
                                   equipmentId,
                                   StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    $"Equipment with ID '{equipmentId}' was not found.");


            return (string)equipment
                        .Element(msl + "specifications")
                        ?.Element(msl + specificationElementName);

        }

        /// <summary>
        /// Gets the SICL interface ID for a network gateway
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        public string InternalResistorValue(string equipmentId)
        {
            XElement equipment =
                        doc.Root?
                           .Elements(msl + "equipment")
                           .FirstOrDefault(e =>
                               string.Equals(
                                   (string)e.Element(msl + "id"),
                                   equipmentId,
                                   StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    $"Equipment with ID '{equipmentId}' was not found.");


            return (string)equipment
                        .Element(msl + "specifications")
                        ?.Element(msl + "sicl");

        }


    }
}
