﻿namespace JobsBgScraper.Common
{
    public static class GlobalConstants
    {
        #region Site Meta Info

        public static readonly string SITE_NAME = "JobsBg";
        public static readonly string SITE_PROBE_URI = "https://www.jobs.bg/front_job_search.php?frompage=0&add_sh=1&categories%5B0%5D=15";
        public static readonly string DEFAULT_CATEGORY_ID = "15";
        public static readonly int DEFAULT_ITEM_COUNT_PER_PAGE = 15;

        #endregion

        #region XPath Site Selectors

        public static readonly string HTML_JOB_CLASS_NAME = "card__title";
        public static readonly string HTML_COMPANY_CLASS_NAME = "company_link";
        public static readonly string HTML_PAGE_LINK_CLASS_NAME = "pathlink";
        public static readonly string HTML_PAGE_LINK_CURRENT_CLASS_NAME = "currentpage";

        #endregion
    }
}
