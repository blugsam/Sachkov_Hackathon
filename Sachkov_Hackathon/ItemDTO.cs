using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachkov_Hackathon
{
    public class ItemDTO
    {
        public record ItemKey(int Id, string Type);
    }
}