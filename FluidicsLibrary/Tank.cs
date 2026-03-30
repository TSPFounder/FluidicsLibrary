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
    public class Tank 
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
        private CAD_Parameter _FillVolume;  //  in³
        private CAD_Parameter _TotalVolume;  //  in³
        private CAD_Parameter _Pressure;
        private CAD_Parameter _MaxPressure;
        private CAD_Parameter _HeadspacePressure;
        private CAD_Parameter _FluidDensity;
        private CAD_Parameter _FluidSpecificHeat;
        private CAD_Parameter _Temperature;
        private CAD_Parameter _InletFlowRate;
        private CAD_Parameter _OutletFlowRate;
        private CAD_Parameter _LowLevelAlarm;
        private CAD_Parameter _HighLevelAlarm;

         //
        //  Owned & Owning Objects

        #endregion

        //  *****************************************************************************************


        //  ****************************************************************************************
        //  INITIALIZATIONS
        //
        //  ************************************************************

        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region

        public enum TankTypeEnum
        {
            Open = 0,
            Pressurized,
            Bladder,
            Accumulator,
            Reservoir,
            Other
        }

        public enum TankShapeEnum
        {
            Cylindrical = 0,
            Rectangular,
            Spherical,
            Other
        }

        public enum TankStatusEnum
        {
            Empty = 0,
            Normal,
            Full,
            Overpressure,
            Faulted
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  TANK CONSTRUCTOR
        //
        //  ************************************************************
        public Tank()
        {
            _FillVolume = new CAD_Parameter();
            _TotalVolume = new CAD_Parameter();
            _Pressure = new CAD_Parameter();
            _MaxPressure = new CAD_Parameter();
            _HeadspacePressure = new CAD_Parameter();
            _FluidDensity = new CAD_Parameter();
            _FluidSpecificHeat = new CAD_Parameter();
            _Temperature = new CAD_Parameter();
            _InletFlowRate = new CAD_Parameter();
            _OutletFlowRate = new CAD_Parameter();
            _LowLevelAlarm = new CAD_Parameter();
            _HighLevelAlarm = new CAD_Parameter();

            Status = TankStatusEnum.Empty;
        }
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  PROPERTIES
        //
        //  ************************************************************
        #region
        //
        //  Identification

        /// <summary>
        /// Type of tank.
        /// </summary>
        public TankTypeEnum TankType { get; set; }

        /// <summary>
        /// Shape of the tank.
        /// </summary>
        public TankShapeEnum TankShape { get; set; }

        /// <summary>
        /// Current operating status.
        /// </summary>
        public TankStatusEnum Status { get; private set; }

        //  
        //  Data

        /// <summary>
        /// Fill volume in in³.
        /// </summary>
        public CAD_Parameter FillVolume
        {
            set => _FillVolume = value;
            get
            {
                return _FillVolume;
            }
        }

        /// <summary>
        /// Total volume in in³.
        /// </summary>
        public CAD_Parameter TotalVolume
        {
            set => _TotalVolume = value;
            get
            {
                return _TotalVolume;
            }
        }

        /// <summary>
        /// Current internal pressure in Pascals.
        /// </summary>
        public CAD_Parameter Pressure
        {
            set => _Pressure = value;
            get
            {
                return _Pressure;
            }
        }

        /// <summary>
        /// Maximum allowable working pressure in Pascals.
        /// </summary>
        public CAD_Parameter MaxPressure
        {
            set => _MaxPressure = value;
            get
            {
                return _MaxPressure;
            }
        }

        /// <summary>
        /// Headspace (gas blanket) pressure for pressurized tanks in Pascals.
        /// </summary>
        public CAD_Parameter HeadspacePressure
        {
            set => _HeadspacePressure = value;
            get
            {
                return _HeadspacePressure;
            }
        }

        /// <summary>
        /// Fluid density in kg/m³.
        /// </summary>
        public CAD_Parameter FluidDensity
        {
            set => _FluidDensity = value;
            get
            {
                return _FluidDensity;
            }
        }

        /// <summary>
        /// Fluid specific heat capacity in J/(kg·K).
        /// </summary>
        public CAD_Parameter FluidSpecificHeat
        {
            set => _FluidSpecificHeat = value;
            get
            {
                return _FluidSpecificHeat;
            }
        }

        /// <summary>
        /// Fluid temperature in °C.
        /// </summary>
        public CAD_Parameter Temperature
        {
            set => _Temperature = value;
            get
            {
                return _Temperature;
            }
        }

        /// <summary>
        /// Inlet flow rate in in³/s.
        /// </summary>
        public CAD_Parameter InletFlowRate
        {
            set => _InletFlowRate = value;
            get
            {
                return _InletFlowRate;
            }
        }

        /// <summary>
        /// Outlet flow rate in in³/s.
        /// </summary>
        public CAD_Parameter OutletFlowRate
        {
            set => _OutletFlowRate = value;
            get
            {
                return _OutletFlowRate;
            }
        }

        /// <summary>
        /// Low-level alarm threshold as a fraction (0.0 to 1.0).
        /// </summary>
        public CAD_Parameter LowLevelAlarm
        {
            set => _LowLevelAlarm = value;
            get
            {
                return _LowLevelAlarm;
            }
        }

        /// <summary>
        /// High-level alarm threshold as a fraction (0.0 to 1.0).
        /// </summary>
        public CAD_Parameter HighLevelAlarm
        {
            set => _HighLevelAlarm = value;
            get
            {
                return _HighLevelAlarm;
            }
        }

        //
        //  Owned & Owning Objects

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************

        //  *****************************************************************************************


        //  *****************************************************************************************
        //  EVENTS
        //
        //  ************************************************************

        //  *****************************************************************************************
    }
}
