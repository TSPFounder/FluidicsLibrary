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
    public class HVAC :SE_System
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region
        //  
        //  Identification

        //
        //  Data
        private Point _Location;
        private CAD_Dimension _Height;
        private CAD_Dimension _Length;
        private CAD_Dimension _Width;
        //
        //  Owned & Owning Objects
        //
        //  My Building
        //private SimulationBuilding _MyBuilding;
        //
        //  Ducts
        private Duct _CurrentDuct;
        private List<Duct> _MyDucts;
        //
        //  Condenser
        private HeatExchanger _MyCondenser;
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

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  HVAC CONSTRUCTOR
        //
        //  ************************************************************
        #region
        public HVAC()
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
        //  Data
        //
        //  Position & Orientation
        public Point Location
        {
            set => _Location = value;
            get
            {
                return _Location;
            }
        }
        //
        //  Dimensions
        public CAD_Dimension Length
        {
            set => _Length = value;
            get
            {
                return _Length;
            }
        }
        public CAD_Dimension Width
        {
            set => _Width = value;
            get
            {
                return _Width;
            }
        }
        public CAD_Dimension Height
        {
            set => _Height = value;
            get
            {
                return _Height;
            }
        }
        //
        //  Owned & Owning Objects
        //
        //  Ducts
        public Duct CurrentDuct
        {
            set => _CurrentDuct = value;
            get
            {
                return _CurrentDuct;
            }
        }
        public List<Duct> MyDucts
        {
            set => _MyDucts = value;
            get
            {
                return _MyDucts;
            }
        }
        //
        //  Condenser
        public HeatExchanger MyCondenser
        {
            set => _MyCondenser = value;
            get
            {
                return _MyCondenser;
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
