using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Temperature_Monitor
{
    class InventoryItem
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

    class CalibrationRecord
    {
        public string EquipmentId { get; set; }
        public string Quantity { get; set; }
        public int CalibrationIntervalYears { get; set; }
        public string ComponentName { get; set; }
        public string ReportId { get; set; }
        public DateTime? ReportIssueDate { get; set; }
        public DateTime MeasurementStartDate { get; set; }
        public DateTime MeasurementStopDate { get; set; }
        public bool HasEquation { get; set; }
        public bool HasTable { get; set; }
        public string EquationUnit { get; set; }

        public string Equation { get; set; }

    }
    class EquipmentRegister
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
                    $"Equipment with ID '{equipmentId}' was not found.");

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
                    $"Equipment with ID '{equipmentId}' was not found.");

            
            InventoryItem i =  new InventoryItem
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
        /// Gets a list of list calibration records for a given piece of equipment
        /// </summary>
        /// <param name="equipmentId">The id of the equipment</param>
        public List<CalibrationRecord> CalData(string equipmentId)
        {
            XElement equipment =
                        doc.Root?
                           .Elements(msl + "equipment")
                           .FirstOrDefault(e =>string.Equals(
                                   (string)e.Element(msl + "id"),
                                   equipmentId,
                                   StringComparison.OrdinalIgnoreCase));

            if (equipment == null)
                throw new InvalidOperationException(
                    $"Equipment '{equipmentId}' not found");

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
                                      .DefaultIfEmpty() // components with no reports

                select new CalibrationRecord
                {
                    EquipmentId = equipmentId,
                    Quantity = (string)measurand.Attribute("quantity"),
                    CalibrationIntervalYears =
                        (int)measurand.Attribute("calibrationInterval"),

                    ComponentName = (string)component.Attribute("name"),

                    ReportId = (string)report?.Attribute("id"),
                    ReportIssueDate = (DateTime?)report?
                                   .Element(msl + "reportIssueDate"),
                    HasEquation = report?.Element(msl + "equation") != null,
                    HasTable = report?.Element(msl + "table") != null
                };

            return records.ToList();


        }

        public CalibrationRecord GetLatestCalibrationMetadata(string equipmentId)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentException("equipmentId must not be empty", nameof(equipmentId));

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

            var reports =
                from measurand in equipment
                    .Element(msl + "calibrations")?
                    .Elements(msl + "measurand")
                    ?? Enumerable.Empty<XElement>()

                from component in measurand.Elements(msl + "component")

                from report in component.Elements()
                    .Where(el =>
                        el.Name == msl + "report" ||
                        el.Name == msl + "digitalReport")

                let startDate =
                    (DateTime?)report.Element(msl + "measurementStartDate")

                where startDate.HasValue

                let stopDate =
                    (DateTime?)report.Element(msl + "measurementStopDate")

                where stopDate.HasValue

                select new
                {
                    Quantity = (string)measurand.Attribute("quantity"),
                    ComponentName = (string)component.Attribute("name"),
                    Report = report,
                    MeasurementStartDate = startDate.Value,
                    MeasurementStopDate = stopDate.Value
                };

            var latest = reports
                .OrderByDescending(r => r.MeasurementStartDate)
                .FirstOrDefault();

            if (latest == null)
                return null; // no calibration data at all

            XElement reportElement = latest.Report;

            return new CalibrationRecord
            {
                EquipmentId = equipmentId,
                Quantity = latest.Quantity,
                ComponentName = latest.ComponentName,

                ReportId = (string)reportElement.Attribute("id"),
                MeasurementStartDate = latest.MeasurementStartDate,
                MeasurementStopDate = latest.MeasurementStopDate,
                ReportIssueDate =
                    (DateTime?)reportElement.Element(msl + "reportIssueDate"),

                HasEquation = reportElement.Element(msl + "equation") != null,
                HasTable = reportElement.Element(msl + "table") != null,

                EquationUnit =
                    (string)reportElement
                        .Element(msl + "equation")
                        ?.Element(msl + "unit")
            };
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
                    $"Equipment '{equipmentId}' was not found.");

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
