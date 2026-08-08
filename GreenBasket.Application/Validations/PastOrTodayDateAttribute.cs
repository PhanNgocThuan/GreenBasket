using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Application.Validations
{
    public class PastOrTodayDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                // Chặn trường hợp client quên gửi field → model binder gán default(DateTime) = 0001-01-01
                if (date == default)
                {
                    return false;
                }

                // Ngày truyền vào phải nhỏ hơn hoặc bằng ngày hiện tại (UTC)
                return date.Date <= DateTime.UtcNow.Date;
            }
            return false;
        }
    }
}