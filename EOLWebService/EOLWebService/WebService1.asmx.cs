using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace EOLWebService
{
    /// <summary>
    /// WebService1 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        /// <summary>
        /// 连接数据库,获取总成条码数据(数据库只有一条数据），对此条总成条码数据中的总成条码及时间进行更新，保存数据库。（保证总成条码的唯一，供系统准确获取总成条码）
        /// </summary>
        /// <param name="Code">总成条码</param>
        /// <returns></returns>
        [WebMethod]
        public bool GetCode(string Code)
        {
            try
            {
                EOLEntities entities = new EOLEntities();//声明连接的数据库（EF数据库实体框架）
                var data = entities.Only.FirstOrDefault();//获取数据库所有总成条码数据
                data.Assemblycode = Code;//修改总成条码的数据
                data.Date = DateTime.Now;//修改时间
                entities.SaveChanges();//保存对于数据库所作的更改
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
