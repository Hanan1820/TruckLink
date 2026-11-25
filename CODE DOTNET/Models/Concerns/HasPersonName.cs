
using TruckLink.Support.Rules;
using TruckLink.Support.ViewComponents;

namespace TruckLink.Models.Concerns;







public interface IHasPersonName
{
    string FirstName { get; set; }
    string LastName { get; set; }
    string Phone { get; set; }

    
    string FullName => $"{FirstName} {LastName}".Trim();

    
    string Initials => Avatar.InitialsOf(FirstName, LastName);

    
    string PhoneDisplay => TruckLinkPhone.Display(Phone);

    
    string PhoneLocal => TruckLinkPhone.Local(Phone);
}
