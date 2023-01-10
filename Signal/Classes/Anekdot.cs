using System.Collections.Generic;
using System;

namespace Signal.Classes
{
    public class Anekdot
    {
        static List<string> Anekdots = new List<string>() {
            "Из комбинации лени и логики получаются программисты",
            "Жил-был программист и было у него два сына - Антон и Неантон",
            "Чем отличается программист от политика? Программисту платят деньги за работающие программы",
            "Работа программиста и шамана имеет много общего - оба бормочут непонятные слова, совершают непонятные действия и не могут объяснить, как оно работает"
        };

        static public string GetRandomAnekdot()
        {
            var random = new Random();
            int index = random.Next(Anekdots.Count);

            return Anekdots[index];
        }
    }
}
