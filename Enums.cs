using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Icebreaker
{
    public static class Enums
    {
        public enum BodyType
        {
            [Description("Rather not say")]
            RatherNotSay,
            Thin,
            Fit,
            Average,
            Jacked,
            Curvy,
            [Description("Full figured")]
            FullFigured,
            [Description("A little extra")]
            ALittleExtra,
            Overweight,
            [Description("Used Up")]
            UsedUp
        }

        public enum Ethnicity
        {
            Asian,
            Black,
            [Description("Hispanic / Latin")]
            HispanicLatin,
            Indian,
            [Description("Middle Eastern")]
            MiddleEastern,
            [Description("Native American")]
            NativeAmerican,
            [Description("Pacific Islander")]
            PacificIslander,
            White
        }

        public enum Gender
        {
            Man,
            Woman,
            Agender,
            Androgynous,
            Bigender,
            [Description("Cis Man")]
            CisMan,
            [Description("Cis Woman")]
            CisWoman,
            Genderfluid,
            Genderqueer,
            [Description("Gender Nonconforming")]
            GenderNonconforming,
            Hijra,
            Intersex,
            [Description("Non-binary")]
            NonBinary,
            Other,
            Pangender,
            Transfeminine,
            Transgender,
            Transmasculine,
            Transsexual,
            [Description("Trans Man")]
            TransMan,
            [Description("Trans Woman")]
            TransWoman,
            [Description("Two Spirits")]
            TwoSpirit
        }

        public enum Orientation
        {
            Straight,
            Gay,
            Bisexual,
            Asexual,
            Demisexual,
            Heteroflexible,
            Homoflexible,
            Lesbian,
            Pansexual,
            Queer,
            Questioning,
            Sapiosexual
        }

        public enum RelationshipStatus
        {
            Single,
            [Description("Seeing Someone")]
            SeeingSomeone,
            Married,
            [Description("Open Relationship")]
            OpenRelationship
        }

        public enum MessageRunType { SingleMessageRun = 1, SingleMessageSearch = 2 }

        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return value.ToString();
        }
    }

    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

}
