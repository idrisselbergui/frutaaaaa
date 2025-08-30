using System.Collections.Generic;

namespace frutaaaaa.Models
{
    public class EcartDetailsResponseDto
    {
        public IEnumerable<EcartDetailsDto> Data { get; set; }
        public decimal TotalPdsfru { get; set; }
       
    }
}
