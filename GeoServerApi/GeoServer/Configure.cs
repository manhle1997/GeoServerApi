namespace GeoServerApi.GeoServer
{
    public enum Configure
    {
        /*
         * Represents null value 
         */
        empty,
        /**
         * Only setup the first feature/coverages type available in the data
         * store/coveragestore. This is the default value.
         **/
        first,
        /**
         * Do not configure any feature types/coverages.
         */
        none,
        /**
         * cnfigure all featuretypes/coverages.
         */
        all
    }
}
