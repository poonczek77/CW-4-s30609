namespace Tutorial3Tests;

using Tutorial3.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;

public class AdvancedEmpDeptTests
{
    // 11. MAX salary
    [Fact]
    public void ShouldReturnMaxSalary()
    {
        var emps = Database.GetEmps();

        decimal? maxSalary = emps.Max(e => e.Sal);

        Assert.Equal(5000, maxSalary);
    }

    // 12. MIN salary in department 30
    [Fact]
    public void ShouldReturnMinSalaryInDept30()
    {
        var emps = Database.GetEmps();

        decimal? minSalary = emps
            .Where(e => e.DeptNo == 30)
            .Min(e => e.Sal);

        Assert.Equal(1250, minSalary);
    }

    // 13. Take first 2 employees ordered by hire date
    [Fact]
    public void ShouldReturnFirstTwoHiredEmployees()
    {
        var emps = Database.GetEmps();

        var firstTwo = emps
            .OrderBy(e => e.HireDate)
            .Take(2)
            .ToList();

        Assert.Equal(2, firstTwo.Count);
        Assert.True(firstTwo[0].HireDate <= firstTwo[1].HireDate);
    }

    // 14. DISTINCT job titles
    [Fact]
    public void ShouldReturnDistinctJobTitles()
    {
        var emps = Database.GetEmps();

        var jobs = emps
            .Select(e => e.Job)
            .Distinct()
            .ToList();

        Assert.Contains("PRESIDENT", jobs);
        Assert.Contains("SALESMAN", jobs);
    }

    // 15. Employees with managers (NOT NULL Mgr)
    [Fact]
    public void ShouldReturnEmployeesWithManagers()
    {
        var emps = Database.GetEmps();

        var withMgr = emps
            .Where(e => e.Mgr.HasValue)
            .ToList();

        Assert.All(withMgr, e => Assert.NotNull(e.Mgr));
    }

    // 16. All employees earn more than 500
    [Fact]
    public void AllEmployeesShouldEarnMoreThan500()
    {
        var emps = Database.GetEmps();

        var result = emps.All(e => e.Sal > 500);

        Assert.True(result);
    }

    // 17. Any employee with commission over 400
    [Fact]
    public void ShouldFindAnyWithCommissionOver400()
    {
        var emps = Database.GetEmps();

        var result = emps.Any(e => e.Comm.HasValue && e.Comm > 400);

        Assert.True(result);
    }

    // 18. Self-join to get employee-manager pairs
    [Fact]
    public void ShouldReturnEmployeeManagerPairs()
    {
        var emps = Database.GetEmps();

        var result = emps
            .Join(emps,
                  e => e.Mgr,
                  m => m.EmpNo,
                  (e, m) => new { Employee = e.EName, Manager = m.EName })
            .ToList();

        Assert.Contains(result, r => r.Employee == "SMITH" && r.Manager == "FORD");
    }

    // 19. Let clause usage (sal + comm)
    [Fact]
    public void ShouldReturnTotalIncomeIncludingCommission()
    {
        var emps = Database.GetEmps();

        var result = emps
            .Select(e => new
            {
                e.EName,
                Total = e.Sal + (e.Comm ?? 0)
            })
            .ToList();

        Assert.Contains(result, r => r.EName == "ALLEN" && r.Total == 1900);
    }

    // 20. Join all three: Emp → Dept → Salgrade
    [Fact]
    public void ShouldJoinEmpDeptSalgrade()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();
        var grades = Database.GetSalgrades();

        var result = emps
            .Join(depts,
                e => e.DeptNo,
                d => d.DeptNo,
                (e, d) => new { e, d })
            .SelectMany(
                ed => grades
                    .Where(s => ed.e.Sal >= s.Losal && ed.e.Sal <= s.Hisal)
                    .Select(s => new
                    {
                        EName = ed.e.EName,
                        DName = ed.d.DName,
                        Grade = s.Grade
                    }))
            .ToList();

        Assert.Contains(result, r => r.EName == "ALLEN" && r.DName == "SALES" && r.Grade == 3);
    }
}
