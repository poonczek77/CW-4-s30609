using Tutorial3.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class EmpDeptSalgradeTests
{
    // 1. Simple WHERE filter
    [Fact]
    public void ShouldReturnAllSalesmen()
    {
        var emps = Database.GetEmps();
        List<Emp> result = emps.Where(e => e.Job == "SALESMAN").ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("SALESMAN", e.Job));
    }

    // 2. WHERE + OrderBy
    [Fact]
    public void ShouldReturnDept30EmpsOrderedBySalaryDesc()
    {
        var emps = Database.GetEmps();
        List<Emp> result = emps
            .Where(e => e.DeptNo == 30)
            .OrderByDescending(e => e.Sal)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.True(result[0].Sal >= result[1].Sal);
    }

    // 3. Subquery using LINQ (IN clause)
    [Fact]
    public void ShouldReturnEmployeesFromChicago()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();
        var chicagoDeptNos = depts.Where(d => d.Loc == "CHICAGO").Select(d => d.DeptNo).ToList();

        List<Emp> result = emps
            .Where(e => chicagoDeptNos.Contains(e.DeptNo))
            .ToList();

        Assert.All(result, e => Assert.Equal(30, e.DeptNo));
    }

    // 4. SELECT projection
    [Fact]
    public void ShouldSelectNamesAndSalaries()
    {
        var emps = Database.GetEmps();

        var result = emps
            .Select(e => new { e.EName, e.Sal })
            .ToList();

        Assert.All(result, r =>
        {
            Assert.False(string.IsNullOrWhiteSpace(r.EName));
            Assert.True(r.Sal > 0);
        });
    }

    // 5. JOIN Emp to Dept
    [Fact]
    public void ShouldJoinEmployeesWithDepartments()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        var result = emps.Join(
                depts,
                e => e.DeptNo,
                d => d.DeptNo,
                (e, d) => new { e.EName, d.DName }
            ).ToList();

        Assert.Contains(result, r => r.DName == "SALES" && r.EName == "ALLEN");
    }

    // 6. Group by DeptNo
    [Fact]
    public void ShouldCountEmployeesPerDepartment()
    {
        var emps = Database.GetEmps();

        var result = emps
            .GroupBy(e => e.DeptNo)
            .Select(g => new { DeptNo = g.Key, Count = g.Count() })
            .ToList();

        Assert.Contains(result, g => g.DeptNo == 30 && g.Count == 2);
    }

    // 7. SelectMany (simulate flattening)
    [Fact]
    public void ShouldReturnEmployeesWithCommission()
    {
        var emps = Database.GetEmps();

        var result = emps
            .Where(e => e.Comm.HasValue)
            .Select(e => new { e.EName, e.Comm })
            .ToList();

        Assert.All(result, r => Assert.NotNull(r.Comm));
    }

    // 8. Join with Salgrade
    [Fact]
    public void ShouldMatchEmployeeToSalaryGrade()
    {
        var emps = Database.GetEmps();
        var grades = Database.GetSalgrades();

        var result = emps
            .SelectMany(e => grades
                .Where(s => e.Sal >= s.Losal && e.Sal <= s.Hisal)
                .Select(s => new { e.EName, s.Grade }))
            .ToList();

        Assert.Contains(result, r => r.EName == "ALLEN" && r.Grade == 3);
    }

    // 9. Aggregation (AVG)
    [Fact]
    public void ShouldCalculateAverageSalaryPerDept()
    {
        var emps = Database.GetEmps();

        var result = emps
            .GroupBy(e => e.DeptNo)
            .Select(g => new { DeptNo = g.Key, AvgSal = g.Average(e => e.Sal) })
            .ToList();

        Assert.Contains(result, r => r.DeptNo == 30 && r.AvgSal > 1000);
    }

    // 10. Complex filter with subquery and join
    [Fact]
    public void ShouldReturnEmployeesEarningMoreThanDeptAverage()
    {
        var emps = Database.GetEmps();

        var result = emps
            .Where(e =>
            {
                var deptAvg = emps
                    .Where(x => x.DeptNo == e.DeptNo)
                    .Average(x => x.Sal);
                return e.Sal > deptAvg;
            })
            .Select(e => e.EName)
            .ToList();

        Assert.Contains("ALLEN", result);
    }
}
