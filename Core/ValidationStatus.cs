#nullable disable
namespace KenshiLib.Core;

public enum ValidationStatus
{
  NotChecked,
  Validating,
  OK,
  Warning,
  Error,
  Critical,
}
