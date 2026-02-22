using FluentAssertions;
using Shared.Core;
using Xunit;

namespace Unit.Features.Common;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_should_ceiling_divide_TotalCount_by_PageSize()
    {
        var result = new PagedResult<string>
        {
            TotalCount = 25,
            PageSize = 10,
            PageNumber = 1
        };
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_when_evenly_divisible_should_return_exact_quotient()
    {
        var result = new PagedResult<string>
        {
            TotalCount = 20,
            PageSize = 10,
            PageNumber = 1
        };
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public void HasPreviousPage_should_be_false_on_first_page()
    {
        var result = new PagedResult<string> { PageNumber = 1, PageSize = 10, TotalCount = 100 };
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_should_be_true_when_PageNumber_gt_1()
    {
        var result = new PagedResult<string> { PageNumber = 2, PageSize = 10, TotalCount = 100 };
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_should_be_true_when_more_pages_exist()
    {
        var result = new PagedResult<string> { PageNumber = 1, PageSize = 10, TotalCount = 25 };
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_should_be_false_on_last_page()
    {
        var result = new PagedResult<string> { PageNumber = 3, PageSize = 10, TotalCount = 25 };
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_should_be_false_when_single_page()
    {
        var result = new PagedResult<string> { PageNumber = 1, PageSize = 10, TotalCount = 5 };
        result.HasNextPage.Should().BeFalse();
    }
}
