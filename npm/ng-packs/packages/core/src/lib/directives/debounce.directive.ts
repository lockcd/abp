import { Directive, ElementRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { fromEvent } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { SubscriptionService } from '../services/subscription.service';

@Directive({
  selector: '[input.debounce]',
  providers: [SubscriptionService],
})
export class InputEventDebounceDirective implements OnInit {
  private el = inject(ElementRef);
  private subscription = inject(SubscriptionService);

  @Input() debounce = 300;

  @Output('input.debounce') readonly debounceEvent = new EventEmitter<Event>();

  ngOnInit(): void {
    const input$ = fromEvent<InputEvent>(this.el.nativeElement, 'input').pipe(
      debounceTime(this.debounce),
    );

    this.subscription.addOne(input$, (event: Event) => {
      this.debounceEvent.emit(event);
    });
  }
}
