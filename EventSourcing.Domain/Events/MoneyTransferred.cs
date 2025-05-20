
namespace EventSourcing.Domain.Events
{
    public record MoneyTransferred(
   string AccountId,
   string ToAccountId,
   decimal Amount,
   string Description) : Event(AccountId);
}
