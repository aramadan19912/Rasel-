using Backend.Infrastructure.Data;
using Domain.Entities.Organization;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.Models;

namespace Infrastructure.Data;

public class OrganizationSeeder
{
    private readonly ApplicationDbContext _context;

    public OrganizationSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if already seeded
        if (await _context.Departments.AnyAsync())
        {
            return; // Already seeded
        }

        // Seed Departments
        var departments = await SeedDepartments();

        // Seed Positions
        var positions = await SeedPositions(departments);

        // Seed Employees
        await SeedEmployees(departments, positions);

        await _context.SaveChangesAsync();
    }

    private async Task<Dictionary<string, Department>> SeedDepartments()
    {
        var departments = new Dictionary<string, Department>();

        // Top-level departments
        var executiveDept = new Department
        {
            DepartmentCode = "EXEC",
            Name = "Executive",
            Description = "Executive Leadership",
            Mission = "Provide strategic direction and leadership for the organization",
            Email = "executive@company.com",
            Phone = "+1-555-0100",
            Location = "Building A, Floor 10",
            OfficeNumber = "A1000",
            CostCenter = "CC-EXEC",
            AnnualBudget = 5000000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        departments["EXEC"] = executiveDept;

        var itDept = new Department
        {
            DepartmentCode = "IT",
            Name = "Information Technology",
            Description = "IT Services and Infrastructure",
            Mission = "Deliver innovative technology solutions to drive business success",
            Email = "it@company.com",
            Phone = "+1-555-0200",
            Location = "Building B, Floor 3",
            OfficeNumber = "B300",
            CostCenter = "CC-IT",
            AnnualBudget = 3000000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        departments["IT"] = itDept;

        var hrDept = new Department
        {
            DepartmentCode = "HR",
            Name = "Human Resources",
            Description = "People and Culture",
            Mission = "Attract, develop, and retain top talent",
            Email = "hr@company.com",
            Phone = "+1-555-0300",
            Location = "Building A, Floor 2",
            OfficeNumber = "A200",
            CostCenter = "CC-HR",
            AnnualBudget = 1500000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        departments["HR"] = hrDept;

        var financeDept = new Department
        {
            DepartmentCode = "FIN",
            Name = "Finance",
            Description = "Financial Management and Accounting",
            Mission = "Ensure financial integrity and support strategic decision-making",
            Email = "finance@company.com",
            Phone = "+1-555-0400",
            Location = "Building A, Floor 3",
            OfficeNumber = "A300",
            CostCenter = "CC-FIN",
            AnnualBudget = 2000000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        departments["FIN"] = financeDept;

        var salesDept = new Department
        {
            DepartmentCode = "SALES",
            Name = "Sales",
            Description = "Sales and Business Development",
            Mission = "Drive revenue growth through customer acquisition and retention",
            Email = "sales@company.com",
            Phone = "+1-555-0500",
            Location = "Building C, Floor 1",
            OfficeNumber = "C100",
            CostCenter = "CC-SALES",
            AnnualBudget = 4000000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        departments["SALES"] = salesDept;

        var marketingDept = new Department
        {
            DepartmentCode = "MKT",
            Name = "Marketing",
            Description = "Marketing and Communications",
            Mission = "Build brand awareness and generate demand",
            Email = "marketing@company.com",
            Phone = "+1-555-0600",
            Location = "Building C, Floor 2",
            OfficeNumber = "C200",
            CostCenter = "CC-MKT",
            AnnualBudget = 2500000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        departments["MKT"] = marketingDept;

        // IT Sub-departments
        var itDevDept = new Department
        {
            DepartmentCode = "IT-DEV",
            Name = "Software Development",
            Description = "Application Development and Engineering",
            Mission = "Build and maintain high-quality software solutions",
            Email = "dev@company.com",
            Phone = "+1-555-0210",
            Location = "Building B, Floor 4",
            OfficeNumber = "B400",
            CostCenter = "CC-IT-DEV",
            AnnualBudget = 1500000,
            IsActive = true,
            ParentDepartment = itDept,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        departments["IT-DEV"] = itDevDept;

        var itInfraDept = new Department
        {
            DepartmentCode = "IT-INFRA",
            Name = "IT Infrastructure",
            Description = "Infrastructure and Operations",
            Mission = "Maintain reliable and secure IT infrastructure",
            Email = "infrastructure@company.com",
            Phone = "+1-555-0220",
            Location = "Building B, Floor 5",
            OfficeNumber = "B500",
            CostCenter = "CC-IT-INFRA",
            AnnualBudget = 1000000,
            IsActive = true,
            ParentDepartment = itDept,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        departments["IT-INFRA"] = itInfraDept;

        await _context.Departments.AddRangeAsync(departments.Values);
        await _context.SaveChangesAsync();

        return departments;
    }

    private async Task<Dictionary<string, Position>> SeedPositions(Dictionary<string, Department> departments)
    {
        var positions = new Dictionary<string, Position>();

        // Executive Positions
        positions["CEO"] = new Position
        {
            PositionCode = "CEO-001",
            Title = "Chief Executive Officer",
            Description = "Overall strategic leadership and management",
            Level = 1,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 250000,
            MaxSalary = 500000,
            Currency = "USD",
            Department = departments["EXEC"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        positions["CTO"] = new Position
        {
            PositionCode = "CTO-001",
            Title = "Chief Technology Officer",
            Description = "Technology strategy and innovation",
            Level = 2,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 200000,
            MaxSalary = 350000,
            Currency = "USD",
            Department = departments["IT"],
            ReportsToPosition = positions["CEO"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        positions["CFO"] = new Position
        {
            PositionCode = "CFO-001",
            Title = "Chief Financial Officer",
            Description = "Financial strategy and management",
            Level = 2,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 180000,
            MaxSalary = 320000,
            Currency = "USD",
            Department = departments["FIN"],
            ReportsToPosition = positions["CEO"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        positions["HR-DIR"] = new Position
        {
            PositionCode = "HR-DIR-001",
            Title = "HR Director",
            Description = "Human resources management and strategy",
            Level = 3,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 120000,
            MaxSalary = 180000,
            Currency = "USD",
            Department = departments["HR"],
            ReportsToPosition = positions["CEO"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // IT Positions
        positions["IT-MGR"] = new Position
        {
            PositionCode = "IT-MGR-001",
            Title = "IT Manager",
            Description = "Manage IT operations and team",
            Level = 4,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 100000,
            MaxSalary = 140000,
            Currency = "USD",
            Department = departments["IT"],
            ReportsToPosition = positions["CTO"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        positions["DEV-MGR"] = new Position
        {
            PositionCode = "DEV-MGR-001",
            Title = "Development Manager",
            Description = "Lead software development teams",
            Level = 4,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 110000,
            MaxSalary = 150000,
            Currency = "USD",
            Department = departments["IT-DEV"],
            ReportsToPosition = positions["CTO"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        positions["SR-DEV"] = new Position
        {
            PositionCode = "SR-DEV-001",
            Title = "Senior Software Developer",
            Description = "Design and develop complex software solutions",
            Level = 5,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 90000,
            MaxSalary = 130000,
            Currency = "USD",
            Department = departments["IT-DEV"],
            ReportsToPosition = positions["DEV-MGR"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        positions["DEV"] = new Position
        {
            PositionCode = "DEV-001",
            Title = "Software Developer",
            Description = "Develop and maintain software applications",
            Level = 6,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 70000,
            MaxSalary = 100000,
            Currency = "USD",
            Department = departments["IT-DEV"],
            ReportsToPosition = positions["SR-DEV"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        positions["JR-DEV"] = new Position
        {
            PositionCode = "JR-DEV-001",
            Title = "Junior Developer",
            Description = "Learn and contribute to software development",
            Level = 7,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 50000,
            MaxSalary = 70000,
            Currency = "USD",
            Department = departments["IT-DEV"],
            ReportsToPosition = positions["DEV"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Sales Positions
        positions["SALES-DIR"] = new Position
        {
            PositionCode = "SALES-DIR-001",
            Title = "Sales Director",
            Description = "Lead sales strategy and team",
            Level = 3,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 130000,
            MaxSalary = 200000,
            Currency = "USD",
            Department = departments["SALES"],
            ReportsToPosition = positions["CEO"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        positions["SALES-MGR"] = new Position
        {
            PositionCode = "SALES-MGR-001",
            Title = "Sales Manager",
            Description = "Manage sales team and territory",
            Level = 4,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 80000,
            MaxSalary = 120000,
            Currency = "USD",
            Department = departments["SALES"],
            ReportsToPosition = positions["SALES-DIR"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        positions["SALES-REP"] = new Position
        {
            PositionCode = "SALES-REP-001",
            Title = "Sales Representative",
            Description = "Drive sales and build customer relationships",
            Level = 6,
            EmploymentType = EmploymentType.FullTime.ToString(),
            MinSalary = 50000,
            MaxSalary = 80000,
            Currency = "USD",
            Department = departments["SALES"],
            ReportsToPosition = positions["SALES-MGR"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Positions.AddRangeAsync(positions.Values);
        await _context.SaveChangesAsync();

        return positions;
    }

    private async Task SeedEmployees(Dictionary<string, Department> departments, Dictionary<string, Position> positions)
    {
        var employees = new Dictionary<string, Employee>();

        // CEO
        var ceo = new Employee
        {
            EmployeeNumber = "EMP-0001",
            FirstName = "John",
            LastName = "Smith",
            Email = "john.smith@company.com",
            WorkPhone = "+1-555-0101",
            MobilePhone = "+1-555-0102",
            DateOfBirth = new DateTime(1970, 5, 15),
            HireDate = new DateTime(2015, 1, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["EXEC"],
            Position = positions["CEO"],
            CommunicationLevel = (int)CommunicationLevel.Executive,
            OfficeLocation = "Building A, Floor 10, Room 1001",
            WorkSite = "Headquarters",
            CurrentSalary = 400000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["CEO"] = ceo;

        // CTO
        var cto = new Employee
        {
            EmployeeNumber = "EMP-0002",
            FirstName = "Sarah",
            LastName = "Johnson",
            Email = "sarah.johnson@company.com",
            WorkPhone = "+1-555-0201",
            MobilePhone = "+1-555-0202",
            DateOfBirth = new DateTime(1975, 8, 22),
            HireDate = new DateTime(2016, 3, 15),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["IT"],
            Position = positions["CTO"],
            Manager = ceo,
            CommunicationLevel = (int)CommunicationLevel.SeniorManagement,
            OfficeLocation = "Building B, Floor 3, Room 301",
            WorkSite = "Headquarters",
            CurrentSalary = 280000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["CTO"] = cto;

        // CFO
        var cfo = new Employee
        {
            EmployeeNumber = "EMP-0003",
            FirstName = "Michael",
            LastName = "Chen",
            Email = "michael.chen@company.com",
            WorkPhone = "+1-555-0401",
            MobilePhone = "+1-555-0402",
            DateOfBirth = new DateTime(1973, 11, 10),
            HireDate = new DateTime(2016, 6, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["FIN"],
            Position = positions["CFO"],
            Manager = ceo,
            CommunicationLevel = (int)CommunicationLevel.SeniorManagement,
            OfficeLocation = "Building A, Floor 3, Room 301",
            WorkSite = "Headquarters",
            CurrentSalary = 260000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["CFO"] = cfo;

        // HR Director
        var hrDir = new Employee
        {
            EmployeeNumber = "EMP-0004",
            FirstName = "Emily",
            LastName = "Davis",
            Email = "emily.davis@company.com",
            WorkPhone = "+1-555-0301",
            MobilePhone = "+1-555-0302",
            DateOfBirth = new DateTime(1978, 3, 25),
            HireDate = new DateTime(2017, 2, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["HR"],
            Position = positions["HR-DIR"],
            Manager = ceo,
            CommunicationLevel = (int)CommunicationLevel.MiddleManagement,
            OfficeLocation = "Building A, Floor 2, Room 201",
            WorkSite = "Headquarters",
            CurrentSalary = 150000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["HR-DIR"] = hrDir;

        // Development Manager
        var devMgr = new Employee
        {
            EmployeeNumber = "EMP-0005",
            FirstName = "David",
            LastName = "Wilson",
            Email = "david.wilson@company.com",
            WorkPhone = "+1-555-0211",
            MobilePhone = "+1-555-0212",
            DateOfBirth = new DateTime(1980, 7, 14),
            HireDate = new DateTime(2017, 9, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["IT-DEV"],
            Position = positions["DEV-MGR"],
            Manager = cto,
            CommunicationLevel = (int)CommunicationLevel.Management,
            OfficeLocation = "Building B, Floor 4, Room 401",
            WorkSite = "Headquarters",
            CurrentSalary = 130000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["DEV-MGR"] = devMgr;

        // Senior Developers
        var srDev1 = new Employee
        {
            EmployeeNumber = "EMP-0006",
            FirstName = "Jennifer",
            LastName = "Martinez",
            Email = "jennifer.martinez@company.com",
            WorkPhone = "+1-555-0213",
            MobilePhone = "+1-555-0214",
            DateOfBirth = new DateTime(1985, 4, 18),
            HireDate = new DateTime(2018, 3, 15),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["IT-DEV"],
            Position = positions["SR-DEV"],
            Manager = devMgr,
            CommunicationLevel = (int)CommunicationLevel.Supervisor,
            OfficeLocation = "Building B, Floor 4, Desk 410",
            WorkSite = "Headquarters",
            CurrentSalary = 110000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["SR-DEV1"] = srDev1;

        var srDev2 = new Employee
        {
            EmployeeNumber = "EMP-0007",
            FirstName = "Robert",
            LastName = "Anderson",
            Email = "robert.anderson@company.com",
            WorkPhone = "+1-555-0215",
            MobilePhone = "+1-555-0216",
            DateOfBirth = new DateTime(1983, 9, 30),
            HireDate = new DateTime(2018, 6, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["IT-DEV"],
            Position = positions["SR-DEV"],
            Manager = devMgr,
            CommunicationLevel = (int)CommunicationLevel.Supervisor,
            OfficeLocation = "Building B, Floor 4, Desk 411",
            WorkSite = "Headquarters",
            CurrentSalary = 115000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["SR-DEV2"] = srDev2;

        // Developers
        var dev1 = new Employee
        {
            EmployeeNumber = "EMP-0008",
            FirstName = "Lisa",
            LastName = "Taylor",
            Email = "lisa.taylor@company.com",
            WorkPhone = "+1-555-0217",
            MobilePhone = "+1-555-0218",
            DateOfBirth = new DateTime(1988, 12, 5),
            HireDate = new DateTime(2019, 4, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["IT-DEV"],
            Position = positions["DEV"],
            Manager = srDev1,
            CommunicationLevel = (int)CommunicationLevel.Staff,
            OfficeLocation = "Building B, Floor 4, Desk 420",
            WorkSite = "Headquarters",
            CurrentSalary = 85000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["DEV1"] = dev1;

        var dev2 = new Employee
        {
            EmployeeNumber = "EMP-0009",
            FirstName = "James",
            LastName = "Thompson",
            Email = "james.thompson@company.com",
            WorkPhone = "+1-555-0219",
            MobilePhone = "+1-555-0220",
            DateOfBirth = new DateTime(1987, 6, 20),
            HireDate = new DateTime(2019, 7, 15),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["IT-DEV"],
            Position = positions["DEV"],
            Manager = srDev2,
            CommunicationLevel = (int)CommunicationLevel.Staff,
            OfficeLocation = "Building B, Floor 4, Desk 421",
            WorkSite = "Headquarters",
            CurrentSalary = 82000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["DEV2"] = dev2;

        // Junior Developers
        var jrDev1 = new Employee
        {
            EmployeeNumber = "EMP-0010",
            FirstName = "Amanda",
            LastName = "White",
            Email = "amanda.white@company.com",
            WorkPhone = "+1-555-0221",
            MobilePhone = "+1-555-0222",
            DateOfBirth = new DateTime(1995, 2, 10),
            HireDate = new DateTime(2023, 1, 15),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["IT-DEV"],
            Position = positions["JR-DEV"],
            Manager = dev1,
            CommunicationLevel = (int)CommunicationLevel.Entry,
            OfficeLocation = "Building B, Floor 4, Desk 430",
            WorkSite = "Headquarters",
            CurrentSalary = 60000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["JR-DEV1"] = jrDev1;

        var jrDev2 = new Employee
        {
            EmployeeNumber = "EMP-0011",
            FirstName = "Kevin",
            LastName = "Brown",
            Email = "kevin.brown@company.com",
            WorkPhone = "+1-555-0223",
            MobilePhone = "+1-555-0224",
            DateOfBirth = new DateTime(1996, 8, 28),
            HireDate = new DateTime(2023, 3, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["IT-DEV"],
            Position = positions["JR-DEV"],
            Manager = dev2,
            CommunicationLevel = (int)CommunicationLevel.Entry,
            OfficeLocation = "Building B, Floor 4, Desk 431",
            WorkSite = "Headquarters",
            CurrentSalary = 58000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["JR-DEV2"] = jrDev2;

        // Sales Team
        var salesDir = new Employee
        {
            EmployeeNumber = "EMP-0012",
            FirstName = "Patricia",
            LastName = "Garcia",
            Email = "patricia.garcia@company.com",
            WorkPhone = "+1-555-0501",
            MobilePhone = "+1-555-0502",
            DateOfBirth = new DateTime(1976, 5, 12),
            HireDate = new DateTime(2017, 8, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["SALES"],
            Position = positions["SALES-DIR"],
            Manager = ceo,
            CommunicationLevel = (int)CommunicationLevel.MiddleManagement,
            OfficeLocation = "Building C, Floor 1, Room 101",
            WorkSite = "Headquarters",
            CurrentSalary = 170000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["SALES-DIR"] = salesDir;

        var salesMgr = new Employee
        {
            EmployeeNumber = "EMP-0013",
            FirstName = "Christopher",
            LastName = "Rodriguez",
            Email = "christopher.rodriguez@company.com",
            WorkPhone = "+1-555-0503",
            MobilePhone = "+1-555-0504",
            DateOfBirth = new DateTime(1982, 10, 8),
            HireDate = new DateTime(2018, 11, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["SALES"],
            Position = positions["SALES-MGR"],
            Manager = salesDir,
            CommunicationLevel = (int)CommunicationLevel.Management,
            OfficeLocation = "Building C, Floor 1, Room 105",
            WorkSite = "Headquarters",
            CurrentSalary = 95000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["SALES-MGR"] = salesMgr;

        var salesRep1 = new Employee
        {
            EmployeeNumber = "EMP-0014",
            FirstName = "Michelle",
            LastName = "Lewis",
            Email = "michelle.lewis@company.com",
            WorkPhone = "+1-555-0505",
            MobilePhone = "+1-555-0506",
            DateOfBirth = new DateTime(1990, 3, 16),
            HireDate = new DateTime(2020, 5, 1),
            EmploymentStatus = EmploymentStatus.Active.ToString(),
            EmploymentType = EmploymentType.FullTime.ToString(),
            Department = departments["SALES"],
            Position = positions["SALES-REP"],
            Manager = salesMgr,
            CommunicationLevel = (int)CommunicationLevel.Staff,
            OfficeLocation = "Building C, Floor 1, Desk 120",
            WorkSite = "Headquarters",
            CurrentSalary = 65000,
            SalaryCurrency = "USD",
            IsActive = true,
            CanReceiveInternalMessages = true,
            RequireManagerApproval = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employees["SALES-REP1"] = salesRep1;

        await _context.Employees.AddRangeAsync(employees.Values);
        await _context.SaveChangesAsync();

        // Add some skills for developers
        await SeedEmployeeSkills(employees);
    }

    private async Task SeedEmployeeSkills(Dictionary<string, Employee> employees)
    {
        var skills = new List<EmployeeSkill>
        {
            // Senior Dev 1 Skills
            new EmployeeSkill
            {
                Employee = employees["SR-DEV1"],
                SkillName = "C#",
                SkillCategory = "Programming Language",
                ProficiencyLevel = "Expert",
                YearsOfExperience = 8,
                LastUsedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EmployeeSkill
            {
                Employee = employees["SR-DEV1"],
                SkillName = "ASP.NET Core",
                SkillCategory = "Framework",
                ProficiencyLevel = "Expert",
                YearsOfExperience = 6,
                LastUsedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EmployeeSkill
            {
                Employee = employees["SR-DEV1"],
                SkillName = "Angular",
                SkillCategory = "Framework",
                ProficiencyLevel = "Advanced",
                YearsOfExperience = 5,
                LastUsedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // Dev 1 Skills
            new EmployeeSkill
            {
                Employee = employees["DEV1"],
                SkillName = "C#",
                SkillCategory = "Programming Language",
                ProficiencyLevel = "Advanced",
                YearsOfExperience = 4,
                LastUsedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EmployeeSkill
            {
                Employee = employees["DEV1"],
                SkillName = "SQL",
                SkillCategory = "Database",
                ProficiencyLevel = "Advanced",
                YearsOfExperience = 3,
                LastUsedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // Junior Dev 1 Skills
            new EmployeeSkill
            {
                Employee = employees["JR-DEV1"],
                SkillName = "C#",
                SkillCategory = "Programming Language",
                ProficiencyLevel = "Intermediate",
                YearsOfExperience = 1,
                LastUsedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EmployeeSkill
            {
                Employee = employees["JR-DEV1"],
                SkillName = "JavaScript",
                SkillCategory = "Programming Language",
                ProficiencyLevel = "Beginner",
                YearsOfExperience = 1,
                LastUsedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _context.EmployeeSkills.AddRangeAsync(skills);
        await _context.SaveChangesAsync();
    }
}
