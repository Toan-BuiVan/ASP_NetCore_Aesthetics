using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponseInvoice_Loggin;
using BE_102024.DataAces.NetCore.Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class InvoiceRepository : BaseApplicationService, IInvoiceRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		private IUserRepository _userRepository;
		private IVouchersRepository _vouchersRepository;
		private IProductsRepository _productsRepository;
		private IServicessRepository _servicessRepository;
		public InvoiceRepository(DB_Context context, IConfiguration configuration,
			IServiceProvider serviceProvider, IUserRepository userRepository, 
			IVouchersRepository vouchersRepository, 
			IProductsRepository productsRepository, 
			IServicessRepository servicessRepository) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
			_userRepository = userRepository;
			_vouchersRepository = vouchersRepository;
			_productsRepository = productsRepository;
			_servicessRepository = servicessRepository;
		}

		public async Task<ResponseInvoice_Loggin> Insert_Invoice(InvoiceRequest insert_)
		{
			var returnData = new ResponseInvoice_Loggin();
			var invoiceOut_Loggin = new List<Invoice_Loggin_Ouput>();
			var invoiceDetailOut_Loggin = new List<InvoiceDetail_Loggin_Ouput>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				Users empployee = null;
				Vouchers vouchers = null;
				var customer = await _userRepository.GetUserByUserID(insert_.CustomerID);
				
				if (insert_.EmployeeID != null)
				{
					empployee = await _userRepository.GetUserByUserID(insert_.EmployeeID);
					if (empployee == null || empployee.TypePerson != "Employee")
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "EmployeeID không tồn tại!";
						return returnData;
					}
				}
				if (customer == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "CustomerID không tồn tại!";
					return returnData;
				}
				if (insert_.VoucherID != null)
				{
					vouchers = await _vouchersRepository.GetVouchersByVouchersID(insert_.VoucherID ?? 0);
					if (vouchers == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "VoucherID không tồn tại!";
						return returnData;
					}
				}
				if (insert_.ProductIDs == null && insert_.ServicesIDs == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Vui lòng chọn ít nhất 1 Product || Servicess!";
					return returnData;
				}
				//1. Tạo hóa đơn
				var newInvoice = new Invoice
				{
					EmployeeID = empployee?.UserID,
					CustomerID = customer.UserID,
					VoucherID = vouchers?.VoucherID,
					Code = vouchers?.Code,
					DiscountValue = vouchers?.DiscountValue,
					DateCreated = DateTime.Now,
					Status = "Pending",
					DeleteStatus = 1,
					Type = "Ouput"
				};
				await _context.Invoice.AddAsync(newInvoice);
				await _context.SaveChangesAsync();
				invoiceOut_Loggin.Add(new Invoice_Loggin_Ouput
				{
					InvoiceID = newInvoice.InvoiceID,
					EmployeeID = newInvoice.EmployeeID,
					CustomerID = newInvoice.CustomerID,
					VoucherID = newInvoice.VoucherID,
					Code = vouchers?.Code ,
					DiscountValue = vouchers?.DiscountValue,
					DateCreated = newInvoice.DateCreated,
					Status = newInvoice.Status,
					DeleteStatus = newInvoice.DeleteStatus,
					Type = newInvoice.Type,
				});

				//2. Tạo chi tiết hóa đơn
				//2.1 Tạo chi tiết hóa đơn khi có Product & Servicess
				if (insert_.ProductIDs != null && insert_.ServicesIDs != null && insert_.ProductIDs.Count > 0 && insert_.ServicesIDs.Count > 0)
				{
					int minCount = Math.Min(insert_.ProductIDs.Count, insert_.ServicesIDs.Count);
					for (int i = 0; i < minCount; i++)
					{
						var productID = insert_.ProductIDs[i];
						var quantityProduct = insert_.QuantityProduct[i];
						var product = await _productsRepository.GetProductsByProductID(productID);
						if (product == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ProductID: {productID} không hợp lệ!";
							return returnData;
						}
						if (quantityProduct <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Quantity ProductID: {productID} không hợp lệ!";
							return returnData;
						}

						var servicesID = insert_.ServicesIDs[i];
						var quantityService = insert_.QuantityServices[i];
						var service = await _servicessRepository.GetServicessByServicesID(servicesID);
						if (service == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ServiceID: {servicesID} không hợp lệ!";
							return returnData;
						}
						if (quantityService <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Quantity ServiceID: {servicesID} không hợp lệ!";
							return returnData;
						}
						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ProductID = product.ProductID,
							ProductName = product.ProductName,
							ServiceID = service.ServiceID,
							ServiceName = service.ServiceName,
							VoucherID = vouchers?.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceProduct = product.SellingPrice,
							PriceService = service.PriceService,
							TotalQuantityProduct = quantityProduct,
							TotalQuantityService = quantityService,
							TotalMoney = (quantityProduct * product.SellingPrice) 
										+ (quantityService * service.PriceService),
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};
						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();
						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ProductID = newInvoiceDetail.ProductID,
							ProductName = newInvoiceDetail.ProductName,
							ServiceID = newInvoiceDetail.ServiceID,
							ServiceName = newInvoiceDetail.ServiceName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceProduct = newInvoiceDetail.PriceProduct,
							PriceService = newInvoiceDetail.PriceService,
							TotalQuantityService = newInvoiceDetail.TotalQuantityService,
							TotalQuantityProduct = newInvoiceDetail.TotalQuantityProduct,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}

					// Nếu có sản phẩm dư, tạo bản ghi riêng
					for (int i = minCount; i < insert_.ProductIDs.Count; i++)
					{
						var productID = insert_.ProductIDs[i];
						var quantityProduct = insert_.QuantityProduct[i];
						var product = await _productsRepository.GetProductsByProductID(productID);
						if (product == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ProductID: {productID} không hợp lệ!";
							return returnData;
						}
						if (quantityProduct <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Quantity ProductID: {productID} không hợp lệ!";
							return returnData;
						}
						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ProductID = product.ProductID,
							ProductName = product.ProductName,
							VoucherID = newInvoice.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceProduct = product.SellingPrice,
							TotalQuantityProduct = quantityProduct,
							TotalMoney = quantityProduct * product.SellingPrice,
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};
						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();
						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ProductID = newInvoiceDetail.ProductID,
							ProductName = newInvoiceDetail.ProductName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceProduct = newInvoiceDetail.PriceProduct,
							TotalQuantityProduct = newInvoiceDetail.TotalQuantityProduct,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}

					// Nếu có dịch vụ dư, tạo bản ghi riêng
					for (int i = minCount; i < insert_.ServicesIDs.Count; i++)
					{
						var servicesID = insert_.ServicesIDs[i];
						var quantityService = insert_.QuantityServices[i];
						var service = await _servicessRepository.GetServicessByServicesID(servicesID);
						if (service == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ServiceID: {servicesID} không hợp lệ!";
							return returnData;
						}
						if (quantityService <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Quantity ServiceID: {servicesID} không hợp lệ!";
							return returnData;
						}

						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ServiceID = service.ServiceID,
							ServiceName = service.ServiceName,
							VoucherID = newInvoice.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceService = service.PriceService,
							TotalQuantityService = quantityService,
							TotalMoney = quantityService * service.PriceService,
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};
						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();
						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ServiceID = newInvoiceDetail.ServiceID,
							ServiceName = newInvoiceDetail.ServiceName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceService = newInvoiceDetail.PriceService,
							TotalQuantityService = newInvoiceDetail.TotalQuantityService,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}
				}



				//2.2 Tạo chi tiết hóa đơn khi có Product không có Services
				// 2.2 Tạo chi tiết hóa đơn khi có Product
				if (insert_.ProductIDs != null && insert_.ProductIDs.Count >= 1 && (insert_.ServicesIDs == null || insert_.ServicesIDs.Count == 0))
				{
					int productCount = insert_.ProductIDs.Count;

					for (int i = 0; i < productCount; i++)
					{
						var productID = insert_.ProductIDs[i];
						var quantity = insert_.QuantityProduct[i];

						var product = await _productsRepository.GetProductsByProductID(productID);
						if (product == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ProductID: {productID} không tồn tại!";
							return returnData;
						}
						if (quantity <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Vui lòng nhập lại số lượng của ProductID: {productID}!";
							return returnData;
						}

						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ProductID = product.ProductID,
							ProductName = product.ProductName,
							VoucherID = newInvoice.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceProduct = product.SellingPrice,
							TotalQuantityProduct = quantity,
							TotalMoney = quantity * product.SellingPrice,
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};

						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();

						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ProductID = newInvoiceDetail.ProductID,
							ProductName = newInvoiceDetail.ProductName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceProduct = newInvoiceDetail.PriceProduct,
							TotalQuantityProduct = newInvoiceDetail.TotalQuantityProduct,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}
				}
				// 2.3 Tạo chi tiết hóa đơn khi có Services
				if (insert_.ServicesIDs != null && insert_.ServicesIDs.Count > 0 && (insert_.ProductIDs == null || insert_.ProductIDs.Count == 0))
				{
					if (insert_.QuantityServices == null || insert_.ServicesIDs.Count != insert_.QuantityServices.Count)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Số lượng dịch vụ không khớp với danh sách dịch vụ.";
						return returnData;
					}

					for (int i = 0; i < insert_.ServicesIDs.Count; i++)
					{
						var servicesID = insert_.ServicesIDs[i];
						var quantity = insert_.QuantityServices[i];

						var servicess = await _servicessRepository.GetServicessByServicesID(servicesID);
						if (servicess == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ServiceID: {servicesID} không tồn tại!";
							return returnData;
						}

						if (quantity <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Vui lòng nhập lại số lượng của ServiceID: {servicesID}!";
							return returnData;
						}

						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ServiceID = servicess.ServiceID,
							ServiceName = servicess.ServiceName,
							VoucherID = insert_.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceService = servicess.PriceService,
							TotalQuantityService = quantity,
							TotalMoney = quantity * servicess.PriceService,
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};

						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();

						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ServiceID = newInvoiceDetail.ServiceID,
							ServiceName = newInvoiceDetail.ServiceName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceService = newInvoiceDetail.PriceService,
							TotalQuantityService = newInvoiceDetail.TotalQuantityService,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = $"Insert_Invoice thành công!";
				returnData.invoiceOut_Loggin = invoiceOut_Loggin;
				returnData.invoiceDetailOut_Loggin = invoiceDetailOut_Loggin;
				return returnData;

			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error in Insert_Invoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public Task<ResponseInvoice_Loggin> Delete_Invoice()
		{
			throw new NotImplementedException();
		}

		public Task<ResponseGetListInvoice> GetList_SearchInvoice()
		{
			throw new NotImplementedException();
		}
	}
}
