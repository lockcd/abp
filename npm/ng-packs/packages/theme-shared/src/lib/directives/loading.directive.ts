import { 
  ComponentFactoryResolver, 
  ComponentRef, 
  Directive, 
  ElementRef, 
  EmbeddedViewRef, 
  HostBinding, 
  Injector, 
  Input, 
  OnDestroy, 
  OnInit, 
  Renderer2, 
  ViewContainerRef, 
  inject 
} from '@angular/core';
import { Subscription, timer } from 'rxjs';
import { take } from 'rxjs/operators';
import { LoadingComponent } from '../components';

@Directive({
  selector: '[abpLoading]',
})
export class LoadingDirective implements OnInit, OnDestroy {
  private elRef = inject<ElementRef<HTMLElement>>(ElementRef);
  private cdRes = inject(ComponentFactoryResolver);
  private injector = inject(Injector);
  private renderer = inject(Renderer2);

  private _loading!: boolean;

  @HostBinding('style.position')
  position = 'relative';

  @Input('abpLoading')
  get loading(): boolean {
    return this._loading;
  }

  set loading(newValue: boolean) {
    setTimeout(() => {
      if (!newValue && this.timerSubscription) {
        this.timerSubscription.unsubscribe();
        this.timerSubscription = null;
        this._loading = newValue;

        if (this.rootNode) {
          this.renderer.removeChild(this.rootNode.parentElement, this.rootNode);
          this.rootNode = null;
        }
        return;
      }

      this.timerSubscription = timer(this.delay)
        .pipe(take(1))
        .subscribe(() => {
          if (!this.componentRef) {
            this.componentRef = this.cdRes
              .resolveComponentFactory(LoadingComponent)
              .create(this.injector);
          }

          if (newValue && !this.rootNode) {
            this.rootNode = (this.componentRef.hostView as EmbeddedViewRef<any>).rootNodes[0];
            this.targetElement?.appendChild(this.rootNode as HTMLDivElement);
          } else if (this.rootNode) {
            this.renderer.removeChild(this.rootNode.parentElement, this.rootNode);
            this.rootNode = null;
          }

          this._loading = newValue;
          this.timerSubscription = null;
        });
    }, 0);
  }

  @Input('abpLoadingTargetElement')
  targetElement: HTMLElement | undefined;

  @Input('abpLoadingDelay')
  delay = 0;

  componentRef!: ComponentRef<LoadingComponent>;
  rootNode: HTMLDivElement | null = null;
  timerSubscription: Subscription | null = null;

  ngOnInit() {
    if (!this.targetElement) {
      const { offsetHeight, offsetWidth } = this.elRef.nativeElement;
      if (!offsetHeight && !offsetWidth && this.elRef.nativeElement.children.length) {
        this.targetElement = this.elRef.nativeElement.children[0] as HTMLElement;
      } else {
        this.targetElement = this.elRef.nativeElement;
      }
    }
  }

  ngOnDestroy() {
    if (this.timerSubscription) {
      this.timerSubscription.unsubscribe();
    }
  }
}
