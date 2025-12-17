using System.ComponentModel.DataAnnotations;

namespace Repositories.CustomValidation;
public class FutureOrTodayDate : ValidationAttribute{

    protected override ValidationResult? IsValid(object? value,ValidationContext validationContext){
        DateTime? date=value as DateTime?;

        if(date==null)
        return new ValidationResult("Date is Required!");

        if(date!=null && DateTime.Now.Date>date)
        return new ValidationResult("Start Date cannot be in the past. It must be today or a future date!");

        return ValidationResult.Success;    
    }
}