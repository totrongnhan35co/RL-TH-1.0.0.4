using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Labels.ProjectLabel
{
    public class ProjectLabel
    {
        public enum LabelType
        {
            Default,
            Nutrifood,
            CaoSuDongNai,
            Woka,
            CenteryIndia,
            THMilk,
            Droco,
            THTrueMilk
        }


        private static LabelType _labelType = LabelType.THTrueMilk;// Set project label here
        public static bool IsNutrifood => _labelType == LabelType.Nutrifood;
        public static bool IsDefault => _labelType == LabelType.Default;
        public static bool IsCaoSuDongNai => _labelType == LabelType.CaoSuDongNai;
        public static bool IsWoka => _labelType == LabelType.Woka;
        public static bool IsCenteryIndia => _labelType == LabelType.CenteryIndia;
        public static bool IsTHTrueMilk => _labelType == LabelType.THTrueMilk;
        public static bool IsTHMilk => _labelType == LabelType.THMilk;
        public static bool IsDroco => _labelType == LabelType.Droco;

    }
}