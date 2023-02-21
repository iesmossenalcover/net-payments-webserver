namespace Application.Common.Models;

public record SelectOptionVm(string Key, string Value);
public record SelectorVm(IEnumerable<SelectOptionVm> Options);