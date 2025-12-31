using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using SE_Library;
using CAD;
using Mathematics;

namespace Fluidics
{
    public class Pipe 
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region
        //
        //  Identification
        private CrossSectionalGeometryTypeEnum _CrossSectionType;
        //
        //  Data
        //
        //  Dimensions
        private CAD_Dimension _Length;
        private CAD_Dimension _Thickness;
        //
        //  Round Tube
        private CAD_Dimension _OuterRadius;
        private CAD_Dimension _InnerRadius;
        //
        //  Rectangular Tube
        private CAD_Dimension _FilletRadius;
        private CAD_Dimension _Width;
        private CAD_Dimension _Height;
        //
        //  Elliptical Tube
        private CAD_Dimension _MajorRadius;
        private CAD_Dimension _MinorRadius;
        //
        //  Owned & Owning Objects
        //
        //  Thread
        private CAD_Feature _Thread;
        #endregion
        //  *****************************************************************************************


        //  ****************************************************************************************
        //  INITIALIZATIONS
        //
        //  ************************************************************
        #region

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region
        public enum CrossSectionalGeometryTypeEnum
        {
            Circular = 0,
            Square,
            Rectangular,
            Elliptical,
            Other
        }
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  PIPE CONSTRUCTOR
        //
        //  ************************************************************
        #region
        public Pipe()
        {
       

        }
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  PROPERTIES
        //
        //  ************************************************************
        #region
        //
        //  Identification
        //
        //  Cross-Section Type
        public CrossSectionalGeometryTypeEnum CrossSectionType
        {
            set => _CrossSectionType = value;
            get
            {
                return _CrossSectionType;
            }
        }
        //
        //  Data
        //
        //  Dimensions
        //
        //  Length
        public CAD_Dimension Length
        {
            set => _Length = value;
            get
            {
                return _Length;
            }
        }
        //
        //  Thickness
        public CAD_Dimension Thickness
        {
            set => _Thickness = value;
            get
            {
                return _Thickness;
            }
        }
        //
        //  Round Tube
        //
        //  Outer Radius
        public CAD_Dimension OuterRadius
        {
            set => _OuterRadius = value;
            get
            {
                return _OuterRadius;
            }
        }
        //
        //  Inner Radius
        public CAD_Dimension InnerRadius
        {
            set => _InnerRadius = value;
            get
            {
                return _InnerRadius;
            }
        }
        //
        //  Rectangular Tube
        //
        //  Fillet Radius
        public CAD_Dimension FilletRadius
        {
            set => _FilletRadius = value;
            get
            {
                return _FilletRadius;
            }
        }
        //
        //  Width
        public CAD_Dimension Width
        {
            set => _Width = value;
            get
            {
                return _Width;
            }
        }
        //
        //  Height
        public CAD_Dimension Height
        {
            set => _Height = value;
            get
            {
                return _Height;
            }
        }
        //
        //  Elliptical Tube
        //
        // Major Radius
        public CAD_Dimension MajorRadius
        {
            set => _MajorRadius = value;
            get
            {
                return _MajorRadius;
            }
        }
        //
        //  Minor Radius
        public CAD_Dimension MinorRadius
        {
            set => _MinorRadius = value;
            get
            {
                return _MinorRadius;
            }
        }
        //
        //  Owned & Owning Objects
        //
        //  Thread
        public CAD_Feature Thread
        {
            set => _Thread = value;
            get
            {
                return _Thread;
            }
        }
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  EVENTS
        //
        //  ************************************************************
        #region

        #endregion
        //  *****************************************************************************************
    }
}
