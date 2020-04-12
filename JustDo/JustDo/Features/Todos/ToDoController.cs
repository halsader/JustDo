using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JustDo.Models;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace JustDo.Features.Todos {
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class ToDoController : ControllerBase {
        private readonly IMediator _mediator;

        public ToDoController(IMediator mediator) {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Create a Todo
        /// </summary>
        /// <remarks>Create a new Todo</remarks>
        /// <param name="command">Create Todo model</param>
        /// <response code="200">ID of created Todo</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="500">Error processing request</response>
        [ApiVersion("1.0")]
        [Produces("application/json")]
        [HttpPost, Route("")]
        [SwaggerOperation("TodoCreate")]
        [SwaggerResponse(statusCode: 200, type: typeof(Guid), description: "ID of created Todo")]
        [SwaggerResponse(statusCode: 400, type: typeof(Dictionary<string, string>), description: "Request validation failed")]
        [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse[]), description: "Error processing request")]
        [SwaggerRequestExample(typeof(Create.Command), typeof(Create.ModelExample))]
        public Task<Guid> CreateTodoAsync([FromBody] Create.Command command) {
            if (command is null) {
                throw new ArgumentNullException(nameof(command));
            }

            return _mediator.Send(command);
        }

        /// <summary>
        /// Update a Todo
        /// </summary>
        /// <remarks>Update an existing Todo</remarks>
        /// <param name="id">Todo ID</param>
        /// <param name="commandData">Update Todo model</param>
        /// <response code="200">Todo updated</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="500">Error processing request</response>
        [ApiVersion("1.0")]
        [Produces("application/json")]
        [HttpPut, Route("{id:guid}")]
        [SwaggerOperation("TodoUpdate")]
        [SwaggerResponse(statusCode: 200, type: typeof(void), description: "Todo updated")]
        [SwaggerResponse(statusCode: 400, type: typeof(Dictionary<string, string>), description: "Request validation failed")]
        [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse[]), description: "Error processing request")]
        [SwaggerRequestExample(typeof(Update.CommandData), typeof(Update.ModelExample))]
        public Task UpdateTodoAsync([FromRoute] Guid id, [FromBody] Update.CommandData commandData) {
            if (commandData is null) {
                throw new ArgumentNullException(nameof(commandData));
            }

            return _mediator.Send(new Update.Command {
                Done = commandData.Done,
                DueDate = commandData.DueDate,
                Id = id,
                Name = commandData.Name,
                Priority = commandData.Priority
            });
        }

        /// <summary>
        /// Delete a Todo
        /// </summary>
        /// <remarks>Delete an existing Todo</remarks>
        /// <param name="id">Todo ID</param>
        /// <response code="200">Todo deleted</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="500">Error processing request</response>
        [ApiVersion("1.0")]
        [Produces("application/json")]
        [HttpDelete, Route("{id:guid}")]
        [SwaggerOperation("TodoDelete")]
        [SwaggerResponse(statusCode: 200, type: typeof(void), description: "Todo deleted")]
        [SwaggerResponse(statusCode: 400, type: typeof(Dictionary<string, string>), description: "Request validation failed")]
        [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse[]), description: "Error processing request")]
        [SwaggerRequestExample(typeof(Guid), typeof(Delete.ModelExample))]
        public Task DeleteTodoAsync([FromRoute] Guid id) {
            return _mediator.Send(new Delete.Command {
                Id = id,
            });
        }

        /// <summary>
        /// Read a single Todo
        /// </summary>
        /// <remarks>Read a single Todo data</remarks>
        /// <param name="id">Todo ID</param>
        /// <response code="200">Todo data</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="500">Error processing request</response>
        [ApiVersion("1.0")]
        [Produces("application/json")]
        [HttpGet, Route("{id:guid}")]
        [SwaggerOperation("TodoDetails")]
        [SwaggerResponse(statusCode: 200, type: typeof(TodoEnvelope), description: "Todo data")]
        [SwaggerResponse(statusCode: 400, type: typeof(Dictionary<string, string>), description: "Request validation failed")]
        [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse[]), description: "Error processing request")]
        [SwaggerRequestExample(typeof(Guid), typeof(Details.ModelExample))]
        [SwaggerResponseExample(200, typeof(Details.ResponseExample))]
        public Task<TodoEnvelope> GetTodoDetailsAsync([FromRoute] Guid id) {
            return _mediator.Send(new Details.Query {
                Id = id,
            });
        }

        /// <summary>
        /// Read a Todo list
        /// </summary>
        /// <remarks>Read a Todo list with optional order and filtering</remarks>
        /// <param name="query">Optional ordering and filtering params</param>
        /// <response code="200">Todo list</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="500">Error processing request</response>
        [ApiVersion("1.0")]
        [Produces("application/json")]
        [HttpPost, Route("query/list")]
        [SwaggerOperation("TodoList")]
        [SwaggerResponse(statusCode: 200, type: typeof(TodoListEnvelope), description: "Todo list")]
        [SwaggerResponse(statusCode: 400, type: typeof(Dictionary<string, string>), description: "Request validation failed")]
        [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse[]), description: "Error processing request")]
        [SwaggerRequestExample(typeof(List.Query), typeof(List.ModelExample))]
        [SwaggerResponseExample(200, typeof(List.ResponseExample))]
        public Task<TodoListEnvelope> GetTodoListAsync([FromBody] List.Query query) => _mediator.Send(query);

        /// <summary>
        /// Read a paged Todo list
        /// </summary>
        /// <remarks>Read a paged Todo list with optional order and filtering</remarks>
        /// <param name="query">Optional ordering and filtering params</param>
        /// <response code="200">Todo paged list</response>
        /// <response code="400">Request validation failed</response>
        /// <response code="500">Error processing request</response>
        [ApiVersion("1.0")]
        [Produces("application/json")]
        [HttpPost, Route("query/paged")]
        [SwaggerOperation("TodoListPaged")]
        [SwaggerResponse(statusCode: 200, type: typeof(TodoPagedListEnvelope), description: "Todo paged list")]
        [SwaggerResponse(statusCode: 400, type: typeof(Dictionary<string, string>), description: "Request validation failed")]
        [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse[]), description: "Error processing request")]
        [SwaggerRequestExample(typeof(PagedList.Query), typeof(PagedList.ModelExample))]
        [SwaggerResponseExample(200, typeof(PagedList.ResponseExample))]
        public Task<TodoPagedListEnvelope> GetTodoPagedListAsync([FromBody] PagedList.Query query) => _mediator.Send(query);
    }
}